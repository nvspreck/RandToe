using Microsoft.AspNet.SignalR;
using RandToeWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RandToeWeb.Hubs
{
    public class GameHostHub : Hub
    {
        private RandToeWebContext db = new RandToeWebContext();

        public void SendMove(string gameId, int player, int x, int y)
        {
            
        }

        public WebGame GetGame(string gameId)
        {
            var query = from serachGame in db.WebGames where serachGame.GameId == gameId select serachGame;
            if (query.Count<WebGame>() == 0)
            {
                return null;
            }
            else
            {
                return query.First<WebGame>();
            }
        }

        public bool CreateGame(string gameId, string userId)
        {
            // Ensure it doesn't already exist
            {
                var query = from serachGame in db.WebGames where serachGame.GameId == gameId select serachGame;
                if(query.Count<WebGame>() != 0)
                {
                    return false;
                }
            }

            // Create the game
            WebGame game = new WebGame()
            {
                GameId = gameId,
                IsOver = false,
                Moves = new List<WebMove>(),
                StartingPlayerId = userId
            };

            // Add it to the db
            db.WebGames.Add(game);
            db.SaveChanges();
            return true;
        }

        public List<string> GetAllGameIds()
        {
            List<string> gameIds = new List<string>();
            var query = from serachGame in db.WebGames select serachGame;
            foreach(WebGame game in query)
            {
                gameIds.Add(game.GameId);
            }
            return gameIds;
        }
    }
}