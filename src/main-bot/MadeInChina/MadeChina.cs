using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using static System.Math;

// ------------------------------------------------------------------
// MadeChina with Circular Prediction and Smart Wall Avoidance
// ------------------------------------------------------------------
public class MadeChina : Bot
{
    double saveDistance = 100;
    double idealDistance = 200;
    int isTracking = 0;
    int turnDirection = 1;
    double angleToEnemy = 0;
    double radarTurnAngle = 0;
    double toleranceAngle = 0;
    int isRammed = 0;

    double lastEnemyHeading = 0;
    double lastEnemyId = -1;

    double wallMargin = 100;

    static void Main(string[] args)
    {
        new MadeChina().Start();
    }

    MadeChina() : base(BotInfo.FromFile("MadeChina.json")) { }

    public override void Run()
    {
        BodyColor = Color.FromArgb(0x99, 0x99, 0x99);
        TurretColor = Color.FromArgb(0x88, 0x88, 0x88);
        RadarColor = Color.FromArgb(0x66, 0x66, 0x66);

        while (IsRunning)
        {
            isTracking = 0;
            if (isRammed == 1)
            {
                SetBack(100);
                SetTurnRight(90);
                isRammed = 0;
            } else {
                if (isTracking == 0)
                {
                    TurnRadarRight(double.PositiveInfinity);
                }
            }


            AvoidWalls();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        isTracking = 1;
        if(e.Energy < 0) return;
        angleToEnemy = Direction + BearingTo(e.X, e.Y);
        radarTurnAngle = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
        toleranceAngle = Min(Atan(36.0 / DistanceTo(e.X, e.Y)), 45);
        radarTurnAngle += (radarTurnAngle < 0 ? -toleranceAngle : toleranceAngle);
        SetTurnRadarLeft(radarTurnAngle);

        double firePower = GetFirePowerForDistance(DistanceTo(e.X, e.Y));
        var predicted = PredictEnemyPositionCircular(e, firePower);
        TurnToFaceTarget(predicted.x, predicted.y);

        SetForward(DistanceTo(e.X, e.Y) - saveDistance);

        if (DistanceTo(e.X, e.Y) <= idealDistance && e.Energy >= 0)
        {
            if (e.Speed < 2){
                Fire(3);
            } else {
                Fire(firePower);
            }
        }

        lastEnemyHeading = e.Direction;
        lastEnemyId = e.ScannedBotId;
    }

    public override void OnHitBot(HitBotEvent e) {
        isRammed = 1;
    }

    public override void OnHitWall(HitWallEvent e)
    {
        SetBack(100);
        SetTurnRight(90);
    }

    private void TurnToFaceTarget(double x, double y)
    {
        var bearing = BearingTo(x, y);
        turnDirection = (bearing >= 0) ? 1 : -1;
        SetTurnLeft(bearing);
    }

    private double GetFirePowerForDistance(double distance)
    {
        if (distance > 180){
            return 1.5;
        }
        else if (distance <= 180 && distance > 0){
            return 2;
        } else {
            return 3;
        }
    }

    private (double x, double y) PredictEnemyPositionCircular(ScannedBotEvent e, double bulletPower)
    {
        double bulletSpeed = 20 - 3 * bulletPower;
        double enemyX = e.X;
        double enemyY = e.Y;
        double enemyHeadingRad = e.Direction * (PI / 180);
        double enemySpeed = e.Speed;

        double headingChange = 0;
        if (lastEnemyId == e.ScannedBotId)
        {
            headingChange = (e.Direction - lastEnemyHeading) * (PI / 180);
        }

        double predictedX = enemyX;
        double predictedY = enemyY;
        double predictedHeading = enemyHeadingRad;
        double time = 0;

        while (true)
        {
            double distance = Sqrt(Pow(predictedX - X, 2) + Pow(predictedY - Y, 2));
            double travelTime = distance / bulletSpeed;
            if (time >= travelTime)
                break;

            predictedX += enemySpeed * Cos(predictedHeading);
            predictedY += enemySpeed * Sin(predictedHeading);
            predictedHeading += headingChange;
            time += 1;
        }

        return (predictedX, predictedY);
    }

    private void AvoidWalls()
    {
        bool nearLeft = X < wallMargin;
        bool nearRight = X > (ArenaWidth - wallMargin);
        bool nearTop = Y > (ArenaHeight - wallMargin);
        bool nearBottom = Y < wallMargin;

        if (nearLeft)
        {
            SetTurnRight(NormalizeRelativeAngle(45 - Direction));
            SetForward(100);
        }
        else if (nearRight)
        {
            SetTurnRight(NormalizeRelativeAngle(135 - Direction));
            SetForward(100);
        }
        else if (nearTop)
        {
            SetTurnRight(NormalizeRelativeAngle(225 - Direction));
            SetForward(100);
        }
        else if (nearBottom)
        {
            SetTurnRight(NormalizeRelativeAngle(315 - Direction));
            SetForward(100);
        }
    }
} 