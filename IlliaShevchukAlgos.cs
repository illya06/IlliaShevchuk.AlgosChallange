using Robot.Common;
using robot = Robot.Common.Robot;
using System;
using System.Collections.Generic;
using System.Linq;
using alg = IlliaShevchuk.AlgosChallange.Algos.Algos;

namespace IlliaShevchuk.AlgosChallange
{
    class IlliaShevchukAlgos : IRobotAlgorithm
    {
        public string Author => "Illia Shevchuk";

        public RobotCommand DoStep(IList<robot> robots, int robotToMoveIndex, Map map)
        {
            Logger.OnLogRound += UpdateRound;
            var variant = Variant.GetInstance();
            var self = robots[robotToMoveIndex];
            var friends = GetOwnRobots(robots, Author);
            var enemies = GetEnemyRobots(robots, Author);
            friends.Remove(self);

            var alg = new alg(map, self, friends, enemies);
            return alg.Action();
        }

        private static List<robot> GetOwnRobots(IList<robot> robots, string author) =>
            robots.Where(robot => robot.OwnerName == author).ToList();

        private static List<robot> GetEnemyRobots(IList<robot> robots, string author) =>
            robots.Where(robot => robot.OwnerName != author).ToList();

        public void UpdateRound(object o, LogRoundEventArgs e)
        {
            Console.WriteLine(e);
            Console.WriteLine(o);
        }
    }
}
