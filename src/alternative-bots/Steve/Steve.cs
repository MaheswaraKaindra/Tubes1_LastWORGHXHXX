using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Steve : Bot
{
    static void Main(string[] args)
    {
        new Steve().Start();
    }

    Steve() : base(BotInfo.FromFile("Steve.json")) { }

    // Helper attributes: supports Steve's survival mechanism
    bool hasBeenHit = false;
    private Random random = new();


    public override void Run()
    {
        // Steve's color configuration.
        BodyColor       = Color.Red;
        BulletColor     = Color.Red;
        RadarColor      = Color.Red;
        ScanColor       = Color.Red;
        TurretColor     = Color.Red;

        // Initialization: Steve is always aware and consciouss, scanning for enemies around him.
        if (RadarTurnRemaining == 0)
        {
                SetTurnRadarRight(Double.PositiveInfinity);
        }
        // Steve hates walls.
        AvoidWall();
        Go();
    }

    // Steve avoid enemies and walls when he sees them.
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Steve's state
        double distance = DistanceTo(e.X, e.Y);
        double energy = e.Energy;
        double speed = e.Speed;
        double enemyDirection = e.Direction;
        double bearing = BearingTo(e.X, e.Y);
        // Enemy bot's state.
        bool isKillable = energy < 20;
        bool isClose = distance < 400;
        bool isSlow = Math.Abs(speed) < 2;
        // Steve moves towards far enemy to avoid other bots and getting flanked.
        if (distance > 500)
        {
            SetTurnLeft(bearing);
            SetForward(50);
        }
        // Steve avoids enemy bot when it's near.
        else if (distance < 300)
        {
            double escapeAngle = GetEscapeAngle(e.X, e.Y);
            SetTurnLeft(NormalizeRelativeAngle(escapeAngle - Direction));
            SetForward(100); 
        }
        // Steve is scared of aggressive bots. He doesn't shoot after getting hit.
        if (hasBeenHit) {
            hasBeenHit = false;
            return;
        }
        // Steve only shoots when it's appropriate
        if (isKillable || !isClose || isSlow)
        {
            // Steve's predictive fire mechanism, increasing accuracy 
            double power = GetBulletPower(distance, energy);
            double bulletSpeed = 20 - 3 * power;
            var (predictedX, predictedY) = GetPredictiveFirePosition(e.X, e.Y, speed, enemyDirection, bulletSpeed);
            double turretAngle = NormalizeRelativeAngle(Direction + BearingTo(predictedX, predictedY) - GunDirection);
            SetTurnGunLeft(turretAngle);
            Fire(power);
        }
    }

    // Steve moves avoiding the enemy and the wall.
    public override void OnHitByBullet(HitByBulletEvent e)
    {
        hasBeenHit = true;
        int turnAngle;

        // Wall distance measure.
        const int safeDistance = 50; 
        bool nearLeft = X < safeDistance;
        bool nearRight = X > 800 - safeDistance;
        bool nearTop = Y > 600 - safeDistance;
        bool nearBottom = Y < safeDistance;
        // Move avoiding the wall.
        if (nearLeft)
            turnAngle = random.Next(0, 90); 
        else if (nearRight)
            turnAngle = random.Next(90, 180); 
        else if (nearTop)
            turnAngle = random.Next(180, 270);
        else if (nearBottom)
            turnAngle = random.Next(270, 360); 
        else
            turnAngle = random.Next(-90, 91); 

        SetTurnRight(turnAngle);

        // Moves even farther when enemy is more aggressive with its bullet.
        double distance = 30 * e.Damage;
        
        // Moves in a random direction. Steve likes predicting but hates getting predicted.
        if (random.Next(0, 2) == 0)
        {
            SetForward(distance);
        }
        else
        {
            SetBack(distance);
        }
    }

    // Steve doesn't like touching walls. He gets away from it ASAP. 
    public override void OnHitWall(HitWallEvent e)
    {
        // Near wall definitions (margin of 100).
        bool nearLeft = X < 100;
        bool nearRight = X > 700;
        bool nearTop = Y > 500;
        bool nearBottom = Y < 100;
        
        double escapeAngle = 0;

        // Move to the right when is near left wall and escape angle directs to the right
        if (nearLeft)
        {
            escapeAngle = 0; 
        }
        // Move to the left when is near left wall and escape angle directs to the left
        else if (nearRight)
        {
            escapeAngle = 180; 
        }
        // Move to the top when is near left wall and escape angle directs to the top
        else if (nearBottom)
        {
            escapeAngle = 90; 
        }
        // Move to the bottom when is near left wall and escape angle directs to the bottom
        else if (nearTop)
        {
            escapeAngle = 270;
        }

        SetTurnRight(escapeAngle);
        SetForward(100);
    }

    // Helper methods.
    private (double, double) GetPredictiveFirePosition(double enemyX, double enemyY, double speed, double directionDeg, double bulletSpeed)
    {
        double directionRad = directionDeg * (Math.PI / 180);
        double predictedX = enemyX;
        double predictedY = enemyY;

        double distance = DistanceTo(enemyX, enemyY);
        double time = distance / bulletSpeed;

        predictedX += speed * time * Math.Cos(directionRad);
        predictedY += speed * time * Math.Sin(directionRad);

        return (predictedX, predictedY);
    }

    // Steve is wallphobic. He avoids the wall everytime he's very near to it.
    private void AvoidWall()
    {
        double margin = 25;
        double x = X, y = Y;
        double fieldWidth = 800, fieldHeight = 600;

        if (x < margin || x > fieldWidth - margin || y < margin || y > fieldHeight - margin)
        {
            SetTurnRight(180);
            SetBack(50);
        }
    }

    // Steve's smart firing power decision, considering enemy's distance.
    private double GetBulletPower(double distance, double enemyEnergy)
    {
        if (enemyEnergy < 20) {
            return 3;
        }
        else if (distance < 200)
        {
            return 2;
        }
        else if (distance < 500)
        {
            return 1.5;
        }
        else 
        {
            return 1;
        }
    }

    // Steve hates enemies and he also hates walls.
    private double GetEscapeAngle(double enemyX, double enemyY)
    {
        // Sets escapeAngle 180 degrees opposite to enemy bot.
        double angleToEnemy = BearingTo(enemyX, enemyY);
        double escapeAngle = NormalizeRelativeAngle(Direction + angleToEnemy + 180); 

        // Near wall definitions (margin of 100).
        bool nearLeft = X < 100;
        bool nearRight = X > 700;
        bool nearTop = Y > 500;
        bool nearBottom = Y < 100;

        // Steve is near the wall.

        // Move to the right when is near left wall and escape angle directs to the right
        if (nearLeft && (escapeAngle >= 90 && escapeAngle <= 270))
        {
            escapeAngle = 0; 
        }
        // Move to the left when is near left wall and escape angle directs to the left
        else if (nearRight && (escapeAngle <= 90 || escapeAngle >= 270))
        {
            escapeAngle = 180; 
        }
        // Move to the top when is near left wall and escape angle directs to the top
        else if (nearBottom && (escapeAngle >= 180 && escapeAngle <= 360))
        {
            escapeAngle = 90; 
        }
        // Move to the bottom when is near left wall and escape angle directs to the bottom
        else if (nearTop && (escapeAngle <= 180))
        {
            escapeAngle = 270;
        }

        // Steve is not near the wall. Move opposing the enemy direction.
        return escapeAngle;
    }
}
