using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// --------------------------------------------------------------------------
// EDI (Energy-Domination Instinct)
// --------------------------------------------------------------------------
// Exploits weak bots by scanning for the lowest energy enemy.
// Prioritizes survival, avoids walls, and fires only when effective.
// Utilizes predictive targeting, random movement, and aggressive follow-up.
// --------------------------------------------------------------------------
public class EDI : Bot
{
    static void Main(string[] args)
    {
        new EDI().Start();
    }

    EDI() : base(BotInfo.FromFile("EDI.json")) { }

    // Helper attributes: EDI's little brain to support weakest enemy searching and random movement.
    private double lowestEnergy = double.MaxValue;
    private double targetX, targetY, targetDistance, targetEnergy, targetSpeed, targetDirection;
    private bool hasTarget = false;
    private Random random = new();

    public override void Run()
    {
        // EDI's color configuration.
        BodyColor       = Color.Black;
        BulletColor     = Color.Black;
        RadarColor      = Color.Black;
        ScanColor       = Color.Black;
        TurretColor     = Color.Black;

        // Initialization: EDI scans for the weakest enemy bot to then lock and fire.
        while (IsRunning)
        {
            // EDI looks for the weakest enemy bot target by rotating 360 degrees.
            hasTarget = false;
            lowestEnergy = double.MaxValue;

            if (RadarTurnRemaining == 0)
                SetTurnRadarRight(double.PositiveInfinity);

            // EDI makes sure to avoid kissing the wall.
            AvoidWall();
            Go();

            // EDI lock and fire after weakest enemy has been scanned and targeted.
            if (hasTarget)
                LockAndFire();
        }
    }

    // EDI scans for the weakest enemy bot target.
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // EDI changes target when he sees weaker enemy.
        if (e.Energy < lowestEnergy)
        {
            // Updates lowest energy information on EDI's little brain.
            lowestEnergy = e.Energy;
            targetEnergy = e.Energy;
            // Updates enemy's coordinate and distance on EDI's little brain.
            targetX = e.X;
            targetY = e.Y;
            targetDistance = DistanceTo(e.X, e.Y);
            // Updates enemy's speed and direction on EDI's little brain.
            targetSpeed = e.Speed;
            targetDirection = e.Direction;
            // Reminds EDI's little brain he's gotten a target for feast.
            hasTarget = true;
        }
    }

    // EDI's locking and predictive fire mechanism.
    private void LockAndFire()
    { 
        // Predicts firing direction.
        double power = GetBulletPower(targetDistance, targetEnergy);
        double bulletSpeed = 20 - 3 * power;
        var (predictedX, predictedY) = GetPredictiveFirePosition(targetX, targetY, targetSpeed, targetDirection, bulletSpeed);
        // Locking mechanism.
        double angleToEnemy = Direction + BearingTo(predictedX, predictedY);
        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
        double extraTurn = Math.Min(Math.Atan(36.0 / targetDistance), 45);
        radarTurn += radarTurn < 0 ? -extraTurn : extraTurn;
        SetTurnRadarLeft(radarTurn);
        // Gun turns towards enemy bot.
        double gun = NormalizeRelativeAngle(angleToEnemy - GunDirection);
        SetTurnGunLeft(gun);
        // Moves towards enemy bot.
        TurnTowardsTarget(predictedX, predictedY);
        SetForward(DistanceTo(predictedX, predictedY) + 2);
        // Fires when appropriate.
        if (GunHeat == 0 && Energy > 1)
            Fire(power);
    }

    // EDI likes to ram. He fires and rams again after ramming.
    public override void OnHitBot(HitBotEvent e)
    {
        Fire(3);
        SetForward(40);
    }

    // EDI doesn't like touching walls. He gets away from it ASAP.
    public override void OnHitWall(HitWallEvent e)
    {
        SetBack(50);
    }

    // EDI goes crazy when hit by bullet. He doesn't like move predictively.
    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // EDI's turn angle, movement distance and movement direction is determined randomly.
        int turnAngle = random.Next(-90, 91);
        int distance = random.Next(50, 151);
        SetTurnRight(turnAngle);
        if (random.Next(0, 2) == 0)
        {
            SetForward(distance);
        }
        else
        {
            SetBack(distance);
        }
    }

    // Helper methods.
    private (double x, double y) GetPredictiveFirePosition(double enemyX, double enemyY, double speed, double directionDeg, double bulletSpeed)
    {
        // EDI's talent to predict fire using physics (just a special formula).
        double directionRad = directionDeg * (Math.PI / 180);
        double predictedX = enemyX;
        double predictedY = enemyY;
        double distance = DistanceTo(enemyX, enemyY);
        double time = distance / bulletSpeed;

        predictedX += speed * time * Math.Cos(directionRad);
        predictedY += speed * time * Math.Sin(directionRad);

        return (predictedX, predictedY);
    }

    // EDI's smart firing power decision, considering enemy's distance and energy.
    private double GetBulletPower(double distance, double energy)
    {
        // Bullet hurts more and slower for closer distances and near-death enemy.
        // Bullet travels faster from farther distance, adapted to predictive fire.
        if ((energy < 20 && distance < 300) || (distance < 200))
        {
            return 3;
        }
        else if (distance <  500)
        {
            return 2;
        }
        else 
        {
            return 1;
        }
    }

    // Turn EDI towards target enemy bot.
    private void TurnTowardsTarget(double x, double y)
    {
        var bearing = BearingTo(x, y);
        SetTurnLeft(bearing);
    }

    // EDI is wallphobic. He avoids the wall everytime he's very near to it.
    private void AvoidWall()
    {
        const double margin = 10;
        const double fieldWidth = 800;
        const double fieldHeight = 600;
        double x = X;
        double y = Y;

        if (x < margin || x > fieldWidth - margin || y < margin || y > fieldHeight - margin)
        {
            SetTurnRight(180);
            SetBack(50);
        }
    }
}
