using Robot.Common;
using robot = Robot.Common.Robot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IlliaShevchuk.AlgosChallange.Algos
{
  class Algos
  {
    private int costToAttack = 20;
    private int costToCreate = 1080;
    private double attackYeald = 0;
    private double stationYeald = 0;

    private Map Map { get; set; }
    private robot Self { get; set; }
    private List<robot> Friends { get; set; }
    private List<robot> Enemies { get; set; }

    public Algos(Map map, robot self, List<robot> friends, List<robot> enemies)
    {
      Map = map;
      Self = self;
      Friends = friends;
      Enemies = enemies;
    }

    public RobotCommand Action()
    {
      var posToMove = SelectBestOption();

      if (ShouldCreateRobot())
      {
        return new CreateNewRobotCommand();
      }
      if (ShouldCollect())
      {
        return new CollectEnergyCommand();
      }
      if (posToMove != null)
      {
        return new MoveCommand() { NewPosition = posToMove };
      }

      return null;
    }

    private Position SelectBestOption()
    {
      var (moveToStation, stationPosition) = ShouldMoveToStation();
      var (attack, enemyPosition) = ShouldAttack();

      if (moveToStation && attack)
      {
        if (stationYeald > attackYeald)
          return stationPosition;
        else
          return enemyPosition;
      }

      if (moveToStation)
        return stationPosition;

      if (attack)
        return enemyPosition;

      return null;
    }

    private bool ShouldCreateRobot()
    {
      var isEnoughEnergy = Self.Energy > costToCreate + costToAttack;
      var isNotTooMany = Friends.Count + 1 < 100;

      if (!isEnoughEnergy || !isNotTooMany)
      {
        return false;
      }

      var ableToGo = (int)Math.Sqrt(Self.Energy);
      var station = GetViableStation(ableToGo);
      int distance = (int)Math.Sqrt(Self.Energy);
      var enemiesNear = Enemies
          .Where(enemy => Math.Abs(enemy.Position.X - Self.Position.X) <= distance && Math.Abs(enemy.Position.Y - Self.Position.Y) <= distance)
          .OrderByDescending(enemy => enemy.Energy)
          .ToList();

      if (station != null || enemiesNear.Count != 0)
      {
        return true;
      }

      return false;
    }

    private (bool, Position) ShouldAttack()
    {
      int costToMove;
      double curentYeald, bestYeald = 0;
      var distance = (int)Math.Sqrt(Self.Energy);
      var enemyMVP = Enemies
          .Where(enemy => Math.Abs(enemy.Position.X - Self.Position.X) <= distance && Math.Abs(enemy.Position.Y - Self.Position.Y) <= distance)
          .OrderByDescending(enemy => enemy.Energy)
          .ThenBy(enemy => FindEnergyForDistance(Self.Position, enemy.Position))
          .ToList();

      if (enemyMVP.Count == 0) { return (false, null); }

      robot easyEnemy = null;
      foreach (var enemy in enemyMVP)
      {
        costToMove = FindEnergyForDistance(Self.Position, enemy.Position) + costToAttack;
        curentYeald = enemy.Energy * 0.3 - costToMove;

        if(costToMove > Self.Energy)
        {
          continue;
        }

        if(!IsAbleToMoveTo(enemy.Position))
        {
          continue;
        }

        if (curentYeald > bestYeald)
        {
          easyEnemy = enemy;
          bestYeald = curentYeald;
        }
      }

      attackYeald = bestYeald;

      if (bestYeald > 0) { return (true, easyEnemy.Position); }

      return (false, null);
    }

    private (bool, Position) ShouldMoveToStation()
    {
      var currentStation = Map.Stations.Where(station => station.Position == Self.Position).ToList();
      if (currentStation.Count == 0)
      {
        var ableToGo = (int)Math.Sqrt(Self.Energy);
        var station = GetViableStation(ableToGo);

        if (station.Position != null)
        {
          return (true, station.Position);
        }

        return (false, null);
      }
      return (false, null);
    }

    private bool ShouldCollect()
    {
      if (Map.GetResource(Self.Position) != null && attackYeald < 60) return true;
      return false;
    }

    private EnergyStation GetViableStation(int distance)
    {
      var stations = Map.GetNearbyResources(Self.Position, distance);
      var sortedByProductivity = stations
          .OrderBy(station => FindEnergyForDistance(Self.Position, station.Position))
          .ThenByDescending(station => station.RecoveryRate)
          .ThenByDescending(station => station.Energy)
          .ToList();

      if (sortedByProductivity.Count == 0)
      {
        return null;
      }

      EnergyStation bestStation = null;
      int costToMove, occupiedBy;
      double curentYeald, bestYeald = 0;

      foreach (var station in sortedByProductivity)
      {
        costToMove = FindEnergyForDistance(Self.Position, station.Position);
        curentYeald = station.Energy + station.RecoveryRate - costToMove;
        occupiedBy = OccupiedByWhom(station.Position);

        if (!IsAbleToMoveTo(station.Position))
        {
          continue;
        }
        if (curentYeald > bestYeald && occupiedBy == 0)
        {
          bestStation = station;
          bestYeald = curentYeald;
        }
      }

      stationYeald = bestYeald;

      return bestStation;
    }

    private int OccupiedByWhom(Position point)
    {
      foreach (var robot in Enemies)
      {
        if (robot.Position == point) return -1;
      }
      foreach (var robot in Friends)
      {
        if (robot.Position == point) return 1;
      }
      return 0;
    }

    private int FindEnergyForDistance(Position a, Position b) =>
        (int)(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

    private bool IsAbleToMoveTo(Position a) =>
      Self.Energy >= FindEnergyForDistance(Self.Position, a);
  }
}
