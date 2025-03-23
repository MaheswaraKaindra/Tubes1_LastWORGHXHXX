using System;
using System.Collections.Generic;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using static System.Math;

// --------------------------------------------------------------------------
// Hari Styles
// --------------------------------------------------------------------------
// Barrages the sectors with the highest bot density.
// When hit, runs away to the lowest populated sector.
// We never learn we've been here before,
// Why are we always stuck and running from
// The bullets, the bullets?
// --------------------------------------------------------------------------

public class HariStyles : Bot
{
    static void Main(string[] args) => new HariStyles().Start();
    public HariStyles() : base(BotInfo.FromFile("HariStyles.json")) { }

    // Konstanta Sektor (12 Sektor 30 Derajat)
    private const int SectorCount = 12;
    private const double SectorAngle = 360.0 / SectorCount;

    // Data Radar Scan
    private Dictionary<int, List<(double x, double y)>> sectors = new();
    private double lastRadarDirection = 0, totalRadarSweep = 0;

    // Clustering
    private int targetSector = -1, ticksSinceLastScan = 0;
    private double targetAngle = 0, gunJitterOffset = 0, gunJitterDirection = 1;
    private const double GunJitterMax = 10, GunJitterStep = 2;

    // Boolean flags
    private bool hasCluster = false, sweepRight = true;

    public override void Run()
    {
        BodyColor = Color.Pink;
        TurretColor = Color.HotPink;
        RadarColor = Color.DeepPink;
        ScanColor = Color.Fuchsia;

        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;

        // Sweep Radar 360 Derajat
        SetTurnRadarRight(double.PositiveInfinity);

        while (IsRunning)
        {
            if (!hasCluster)
            { 
                totalRadarSweep += Abs(NormalizeAngle(RadarDirection - lastRadarDirection));
                lastRadarDirection = RadarDirection;

                if (totalRadarSweep >= 360)
                {
                    LockOn();
                }
            }
            else
            {
                SprayAndPray();
                ticksSinceLastScan++;
                if (ticksSinceLastScan > 10)
                {
                    ResetScans();
                }
            }

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double absAngle = NormalizeAngle(Direction + BearingTo(e.X, e.Y));
        int sector = (int)((absAngle + 180) / SectorAngle) % SectorCount;

        if (!hasCluster)
        {
            // Listing Cluster
            if (!sectors.ContainsKey(sector))
                sectors[sector] = new List<(double x, double y)>();

            sectors[sector].Add((e.X, e.Y));
        }
        else
        {
            // Lock Cluster hingga seluruh musuh tidak terlihat di radar
            if (DistanceTo(e.X, e.Y) < 1000)
                ticksSinceLastScan = 0;
        }
    }

    private void LockOn()
    {
        // Pilih Cluster terpadat
        int maxCount = 0;
        foreach (var kvp in sectors)
        {
            if (kvp.Value.Count > maxCount)
            {
                maxCount = kvp.Value.Count;
                targetSector = kvp.Key;
            }
        }

        // Arahin Radar ke Cluster
        targetAngle = -180 + SectorAngle * targetSector + SectorAngle / 2;
        hasCluster = true;
    }

    private void SprayAndPray()
    {
        // Osilasi Radar 45 derajat
        double radarTarget = targetAngle + (sweepRight ? SectorAngle / 2 : -SectorAngle / 2);
        SetTurnRadarLeft(NormalizeAngle(radarTarget - RadarDirection));
        sweepRight = !sweepRight;

        // Tembak ke seluruh area yang dapat dijangkau (maximal gerak gun 10)
        gunJitterOffset += gunJitterDirection * GunJitterStep;
        if (Abs(gunJitterOffset) > GunJitterMax)
        {
            gunJitterDirection *= -1;
        }

        double gunAngle = targetAngle + gunJitterOffset;
        double gunTurn = NormalizeAngle(gunAngle - GunDirection);
        SetTurnGunLeft(gunTurn);

        // Tembak sekuat tenaga (3 atau sampe energi abis)
        if(Energy > 3){
            SetFire(3);
        } else {
            Fire(Energy);
        }
    }

    private void ResetScans()
    {
        hasCluster = false;
        sectors.Clear();
        totalRadarSweep = 0;
        ticksSinceLastScan = 0;
        SetTurnRadarRight(double.PositiveInfinity);
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        MoveToLeastPopulatedSector();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        MoveToLeastPopulatedSector();
    }

    private void MoveToLeastPopulatedSector()
    {
        // Cari Sector dengan bot paling sedikit
        int minCount = 9999;
        int leastPopulatedSector = -1;
        foreach (var kvp in sectors)
        {
            if (kvp.Value.Count < minCount)
            {
                minCount = kvp.Value.Count;
                leastPopulatedSector = kvp.Key;
            }
        }

        if (leastPopulatedSector != -1)
        {
            // Pergi ke tengah sektor
            double targetAngle = -180 + SectorAngle * leastPopulatedSector + SectorAngle / 2;
            double moveAngle = NormalizeAngle(targetAngle - Direction);
            SetTurnRight(moveAngle);
            SetForward(80); // Jalan 100 pixel
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // Ngga ngapa ngapain ini mah
        return;
    }

    private double NormalizeAngle(double angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

}
