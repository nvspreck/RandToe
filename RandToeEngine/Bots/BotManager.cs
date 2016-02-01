using RandToeEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.Bots
{
    public class BotManager
    {
        /// <summary>
        /// The list of bots that can be used.
        /// </summary>
        private static readonly Type[] m_bots = { typeof(RandBot), typeof(ThinkDeep) };

        /// <summary>
        /// Returns a list of bot names that can be used.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetBotNames()
        {
            List<string> names = new List<string>();
            foreach(Type bot in m_bots)
            {
                names.Add(bot.Name);
            }
            return names;
        }

        /// <summary>
        /// Creates a bot given a bot name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IPlayer GetBot(string name)
        {
            foreach (Type bot in m_bots)
            {
                if(name.Equals(bot.Name))
                {
                    return (IPlayer)Activator.CreateInstance(bot);
                }
            }
            return null;
        }
    }
}
