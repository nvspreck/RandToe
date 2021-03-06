﻿using RandToe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToe
{
    public interface IPlayer
    {
        /// <summary>
        /// Called when the player should make a move.
        /// </summary>
        /// <param name="playerBase"></param>
        void MoveRequested(IPlayerBase playerBase);
    }
}
