using RandToe;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RandToe
{
    public class ThinkDeep : IPlayer
    {
        /// <summary>
        /// The value we play
        /// </summary>
        sbyte m_myPlayValue;

        /// <summary>
        /// The value they play.
        /// </summary>
        sbyte m_theirPlayValue;

        /// <summary>
        /// The number of moves checked.
        /// </summary>
        int m_numberOfMovesChecked = 0;

        /// <summary>
        /// The estimated time per move.
        /// </summary>
        double m_averageTimePerMove = 0;

        /// <summary>
        /// Called when we need to make move
        /// </summary>
        /// <param name="playerBase"></param>
        public void MoveRequested(IPlayerBase playerBase)
        {
            // Setup
            m_myPlayValue = playerBase.PlayerId;
            m_theirPlayValue = m_myPlayValue == 1 ? (sbyte)2 : (sbyte)1;

            // Special case, if this is the first move pick the middle middle
            if(playerBase.HistoricMoves.Count == 0)
            {
                playerBase.MakeMove(new PlayerMove(4,4));
                return;
            }

            // Grab the time
            m_numberOfMovesChecked = 0;
            DateTime startTime = DateTime.Now;            

            // Find the move.
            sbyte[][] slots = playerBase.CurrentBoard.Slots;
            // Todo restore depth when I can thread.
            PlayerMove move = ComputeMove(ref slots, playerBase.CurrentBoard.MacroBoardStates, 4);

            // Grab the end time
            TimeSpan diff = DateTime.Now - startTime;

            // Compute time per move.
            double timePerValue = diff.TotalMilliseconds / m_numberOfMovesChecked;

            // Update the average
            if (m_averageTimePerMove == 0)
            {
                m_averageTimePerMove = timePerValue;
            }
            else
            {
                m_averageTimePerMove = (m_averageTimePerMove * .90) + (timePerValue * .10);
            }

            PlayerBase.Logger.Log(this, "Total move calcuation done; time ("+diff.TotalMilliseconds+"ms), computed values ("+m_numberOfMovesChecked+"), time per value ("+timePerValue+"), new average("+m_averageTimePerMove+")");

            // Make the move
            playerBase.MakeMove(move);
        }

        #region Depth First

        private PlayerMove ComputeMove(ref sbyte[][] slots, sbyte[] macroboard, int maxDepth)
        {
            int bestScore = int.MinValue;
            List<PlayerMove> bestMoves = new List<PlayerMove>();
            int numberOfTasks = 0;
            int numberOfTasksDone = 0;
            ManualResetEvent doneEvent = new ManualResetEvent(false);

            // Loop through each macroboard we can play on.
            for(int macroBoardCount = 0; macroBoardCount < 9; macroBoardCount++)
            {
                // If it is playable...
                if(macroboard[macroBoardCount] == -1)
                {
                    // Compute the offset.
                    int xOffset = (int)macroBoardCount % 3 * 3;
                    int yOffset = (int)Math.Floor(macroBoardCount / 3.0) * 3;

                    // Loop through it's spaces.
                    for (int y = 0; y < 3; y++)
                    {
                        for(int x = 0; x < 3; x++)
                        {
                            int macroX = x + xOffset;
                            int macroY = y + yOffset;                           

                            // If the slot isn't taken...
                            if (slots[macroX][macroY] == 0)
                            {
                                // Make a copy of the data for this thread.
                                sbyte[][] localSlots = new sbyte[9][];
                                for(int b = 0; b < 9; b++)
                                {
                                    localSlots[b] = new sbyte[9];
                                    Array.Copy(slots[b], localSlots[b], 9);
                                }
                                sbyte[] localMacroblockState = new sbyte[9];
                                Array.Copy(macroboard, localMacroblockState, 9);
                                int localX = x;
                                int localY = y;
                                int localMacroBlockCount = macroBoardCount;
                                int localMacroX = macroX;
                                int localMacroY = macroY;

                                // Put the work in a task to be executed.
                                numberOfTasks++;

                                // Todo enable threading.
                                //Task.Run(() =>
                                {
                                    // Setup up
                                    int moveScore = 0;
                                    DateTime begin = DateTime.Now;

                                    // Do the move logic.
                                    DoMoveLogic(ref localSlots, ref localMacroblockState, ref moveScore, localX, localY, localMacroBlockCount, true, 0, maxDepth);            

                                    // Get the time now.
                                    DateTime end = DateTime.Now;
                                    TimeSpan diff = end - begin;                     

                                    // Log
                                    PlayerBase.Logger.Log(this, "Move ("+localX+","+localY+")["+localMacroX+","+localMacroY+"] Computed; Score ("+moveScore+"), Time ("+diff.TotalMilliseconds+"ms)");

                                    // If this is equal to the current top score pick at random.
                                    lock(bestMoves)
                                    {
                                        if (moveScore == bestScore)
                                        {
                                            bestMoves.Add(new PlayerMove(localMacroX, localMacroY));
                                            bestScore = moveScore;
                                        }
                                        // If this score is better clear the past list and add this one.
                                        else if (moveScore > bestScore)
                                        {
                                            bestMoves.Clear();
                                            bestMoves.Add(new PlayerMove(localMacroX, localMacroY));
                                            bestScore = moveScore;
                                        }

                                        // Set that we are done
                                        numberOfTasksDone++;
                                        doneEvent.Set();
                                    }
                                }//);
                            }
                        }
                    }
                }
            }

            // Wait for the tasks to finish.
            while(numberOfTasks != numberOfTasksDone)
            {
                doneEvent.WaitOne(50);
            }

            // Pick the first move.
            PlayerMove selectedMove = null;

            // If we have many pick one at random. This will help make us less predictable.
            if(bestMoves.Count > 1)
            {
                // Check for a center value, if we have one play it
                foreach(PlayerMove move in bestMoves)
                {
                    PlayerBase.Logger.Log(this, "Best Move Tied ("+move.MacroX+","+move.MacroY+"); score ("+bestScore+")");

                    if (move.MacroX == 1 && move.MacroY == 1)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 4 && move.MacroY == 1)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 7 && move.MacroY == 1)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 1 && move.MacroY == 4)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 4 && move.MacroY == 4)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 7 && move.MacroY == 4)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 1 && move.MacroY == 7)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 4 && move.MacroY == 7)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 7 && move.MacroY == 7)
                    {
                        selectedMove = move;
                        break;
                    }
                }

                // If we didn't find a center, pick one at random
                if (selectedMove == null)
                {
                    Random rand = new Random();
                    selectedMove = bestMoves[rand.Next(0, bestMoves.Count)];
                }
            }

            // If we don't have a move yet pick the first one.
            if (selectedMove == null)
            {
                selectedMove = bestMoves.First<PlayerMove>();
            }

            PlayerBase.Logger.Log(this, "Best Move Selected ("+selectedMove.MacroX+","+selectedMove.MacroY+"); score ("+bestScore+")");

            return selectedMove;
        }

        private int RecursiveMove(ref sbyte[][] slots, sbyte[] macroboard, bool isMyTurn, int currentDepth, int maxDepth)
        {
            // If we hit the max depth return.
            if(currentDepth > maxDepth)
            {
                return 0;
            }

            // Hold the current score for these moves
            int scoreForMove = 0;

            // Loop though all macroblocks
            for (int macroBlock = 0; macroBlock < 9; macroBlock++)
            {
                // If it is playable
                if (macroboard[macroBlock] == -1)
                {
                    // Loop through the spaces
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            // Try to make a move
                            DoMoveLogic(ref slots, ref macroboard, ref scoreForMove, x, y, macroBlock, isMyTurn, currentDepth, maxDepth);
                        }
                    }
                }
            }

            // When we are done return the score.
            return scoreForMove;
        }

        private bool DoMoveLogic(ref sbyte[][] slots, ref sbyte[] macroboard, ref int scoreForMove, int microX, int microY, int macroblockNumber, bool isMyTurn, int currentDepth, int maxDepth)
        {
            // Indicate we have checked a move.
            m_numberOfMovesChecked++;

            // Compute the offset.
            int xOffset = (int)macroblockNumber % 3 * 3;
            int yOffset = (int)Math.Floor(macroblockNumber / 3.0) * 3;
            int macroX = microX + xOffset;
            int macroY = microY + yOffset;

            // Check to make sure we can play this space.
            if (slots[macroX][macroY] != 0)
            {
                return false;
            }

            // Make the move
            sbyte playedValue = (sbyte)(isMyTurn ? m_myPlayValue : m_theirPlayValue);
            slots[macroX][macroY] = playedValue;

            // Make a copy of the macroboard so we can update it.
            sbyte[] newMacroBoard = new sbyte[9];
            Array.Copy(macroboard, newMacroBoard, 9);

            // Check for a new micro win, note the only new win can be this player id since
            // it is the only thing that changed. 

            // Check for the first row, first col, and the first diag
            bool microWinFound = false;
            if (slots[xOffset][yOffset] == playedValue)
            {
                // Row
                if (slots[xOffset + 1][yOffset] == playedValue)
                {
                    if (slots[xOffset + 2][yOffset] == playedValue)
                    {
                        microWinFound = true;
                    }
                }
                // Diag
                if (slots[xOffset + 1][yOffset + 1] == playedValue)
                {
                    if (slots[xOffset + 2][yOffset + 2] == playedValue)
                    {
                        microWinFound = true;
                    }
                }
                // Col
                if (slots[xOffset][yOffset + 1] == playedValue)
                {
                    if (slots[xOffset][yOffset + 2] == playedValue)
                    {
                        microWinFound = true;
                    }
                }
            }

            // Check for mid col
            if (slots[xOffset + 1][yOffset] == playedValue)
            {
                if (slots[xOffset + 1][yOffset + 1] == playedValue)
                {
                    if (slots[xOffset + 1][yOffset + 2] == playedValue)
                    {
                        microWinFound = true;
                    }
                }
            }

            // Check for the last col
            if (slots[xOffset + 2][yOffset] == playedValue)
            {
                if (slots[xOffset + 2][yOffset + 1] == playedValue)
                {
                    if (slots[xOffset + 2][yOffset + 2] == playedValue)
                    {
                        microWinFound = true;
                    }
                }
            }

            // Check for middle row
            if (slots[xOffset][yOffset + 1] == playedValue)
            {
                if (slots[xOffset + 1][yOffset + 1] == playedValue)
                {
                    if (slots[xOffset + 2][yOffset + 1] == playedValue)
                    {
                        microWinFound = true;
                    }
                }
            }

            // Check for bottom row, and other diag
            if (slots[xOffset][yOffset + 2] == playedValue)
            {
                if (slots[xOffset + 1][yOffset + 2] == playedValue)
                {
                    if (slots[xOffset + 2][yOffset + 2] == playedValue)
                    {
                        microWinFound = true;
                    }
                }
                if (slots[xOffset + 1][yOffset + 1] == playedValue)
                {
                    if (slots[xOffset + 2][yOffset] == playedValue)
                    {
                        microWinFound = true;
                    }
                }
            }

            if (microWinFound)
            {
                // If we have a winner now set it.
                newMacroBoard[macroblockNumber] = playedValue;

                // Calculate the scale for this depth
                double scoreScale = (isMyTurn ? 1 : -1) * Math.Pow(2, currentDepth);

                // Special case, if the depth is 0 this is our move. If we can win do it.
                if (currentDepth == 0 && isMyTurn)
                {
                    // If so, set the scale to be small so the odds we pick this move are very large.
                    scoreScale = .0005;
                }
                // If the depth is 1 it is their turn, if we found a win make this path look bad.
                else if (currentDepth == 1 && !isMyTurn)
                {
                    scoreScale = -.001;
                }

                // Update the score for this round,
                // just a small indication that this is good or bad.
                scoreForMove += (int)(100 / scoreScale);

                // Now check for a macro win.
                bool hasMacroWin = false;
                if (newMacroBoard[0] == playedValue)
                {
                    if (newMacroBoard[1] == playedValue)
                    {
                        if (newMacroBoard[2] == playedValue)
                        {
                            hasMacroWin = true;
                        }
                    }
                    if (newMacroBoard[4] == playedValue)
                    {
                        if (newMacroBoard[8] == playedValue)
                        {
                            hasMacroWin = true;
                        }
                    }
                    if (newMacroBoard[3] == playedValue)
                    {
                        if (newMacroBoard[6] == playedValue)
                        {
                            hasMacroWin = true;
                        }
                    }
                }

                if (newMacroBoard[1] == playedValue)
                {
                    if (newMacroBoard[4] == playedValue)
                    {
                        if (newMacroBoard[7] == playedValue)
                        {
                            hasMacroWin = true;
                        }
                    }
                }

                if (newMacroBoard[2] == playedValue)
                {
                    if (newMacroBoard[5] == playedValue)
                    {
                        if (newMacroBoard[8] == playedValue)
                        {
                            hasMacroWin = true;
                        }
                    }
                }

                if (newMacroBoard[3] == playedValue)
                {
                    if (newMacroBoard[4] == playedValue)
                    {
                        if (newMacroBoard[5] == playedValue)
                        {
                            hasMacroWin = true;
                        }
                    }
                }

                if (newMacroBoard[6] == playedValue)
                {
                    if (newMacroBoard[7] == playedValue)
                    {
                        if (newMacroBoard[8] == playedValue)
                        {
                            hasMacroWin = true;
                        }
                    }
                    if (newMacroBoard[4] == playedValue)
                    {
                        if (newMacroBoard[2] == playedValue)
                        {
                            hasMacroWin = true;
                        }
                    }
                }

                if (hasMacroWin)
                {
                    // Update the score to indicate someone won.
                    scoreForMove += (int)(10000 / scoreScale);

                    // Special case, if the depth is 0 this is our move. If we can win do it.
                    if (currentDepth == 0 && isMyTurn)
                    {
                        scoreForMove = int.MaxValue;
                    }
                    // If the depth is 1 it is their turn. We never want to send them somewhere
                    // they can win.
                    else if (currentDepth == 1 && !isMyTurn)
                    {
                        scoreScale = int.MinValue;
                    }

                    // Reset the macroboard slot
                    slots[macroX][macroY] = 0;

                    // Return now, since they would make this move.
                    return true;
                }
            }

            // If we didn't win the game we need to update the macro boards, figure out where we were sent.
            int sendToMicroBoardIndex = microX + microY * 3;

            // Figure out what our replace value is, if the board is won we will replace with -1.
            sbyte newReplaceValue = (newMacroBoard[sendToMicroBoardIndex] == 1 || newMacroBoard[sendToMicroBoardIndex] == 2) ? (sbyte)-1 : (sbyte)0;

            // Loop through the values
            for (int c = 0; c < 9; c++)
            {
                // If the board isn't won
                if (newMacroBoard[c] != 1 && newMacroBoard[c] != 2)
                {
                    // If we are at the board we are looking for.
                    if (c == sendToMicroBoardIndex)
                    {
                        newMacroBoard[c] = -1;
                    }
                    else
                    {
                        // Set the default value
                        newMacroBoard[c] = newReplaceValue;
                    }
                }
            }

            // Make the move
            int nextDepth = currentDepth + 1;
            scoreForMove += RecursiveMove(ref slots, newMacroBoard, !isMyTurn, nextDepth, maxDepth);

            // Reset the board
            slots[macroX][macroY] = 0;

            return false;
        }

        #endregion

        #region Breadth first

        private class BreadthNode
        {
            public BreadthNode Next;
            public sbyte[][] Slots;
            public sbyte[] MacroboardState;
            public sbyte MacroMoveX;
            public sbyte MacroMoveY;
            public sbyte MovePlayerId;
            public sbyte InitalMoveRoot;
        }


        private PlayerMove ComputeMoveBreadth(MacroBoard board)
        {
            // First setup the first list.
            List<PlayerMove> possibleMoves = board.GetAllPossibleMoves();

            // Setup the initial lists.
            BreadthNode initalList = null;
            int[] moveScoreList = new int[possibleMoves.Count];

            sbyte moveCount = 0;
            foreach(PlayerMove move in possibleMoves)
            {
                // Make a new node
                BreadthNode newNode = new BreadthNode();

                // Copy the slots.
                newNode.Slots = new sbyte[9][];
                for(int x = 0; x < 9; x++)
                {
                    newNode.Slots[x] = new sbyte[9];
                    Array.Copy(board.Slots[x], newNode.Slots[x], 9);
                }

                // Copy the macroboard state
                newNode.MacroboardState = new sbyte[9];
                Array.Copy(board.MacroBoardStates, newNode.MacroboardState, 9);

                // Set the initial move
                newNode.InitalMoveRoot = moveCount++;

                // Set the move
                newNode.MacroMoveX = (sbyte)move.MacroX;
                newNode.MacroMoveY = (sbyte)move.MacroY;
                newNode.MovePlayerId = m_myPlayValue;

                // Set the node into the list.
                if(initalList == null)
                {
                    initalList = newNode;
                }
                else
                {
                    newNode.Next = initalList;
                    initalList = newNode;
                }
            }

            DateTime begin = DateTime.Now;
            m_numberOfMovesChecked = 0;

            // Now our initial list is ready, start the move computation.
            ComputeMoveBreadth(initalList, ref moveScoreList);

            PlayerBase.Logger.Log(this, "moves checked ("+m_numberOfMovesChecked+") time ("+(DateTime.Now - begin).TotalMilliseconds+") time per move ("+(DateTime.Now - begin).TotalMilliseconds / m_numberOfMovesChecked+")");


            // Now pick the best move based on the scores
            List<PlayerMove> bestMoves = new List<PlayerMove>();
            int currentBestScore = int.MinValue;
            for(int c = 0; c < possibleMoves.Count; c++)
            {
                // If the score is the same add it to the list.
                if(moveScoreList[c] == currentBestScore)
                {
                    bestMoves.Add(possibleMoves[c]);
                }
                // If the score is higher set this to be new best.
                else if(moveScoreList[c] > currentBestScore)
                {
                    bestMoves.Clear();
                    bestMoves.Add(possibleMoves[c]);
                    currentBestScore = moveScoreList[c];
                }
            }

            // Now pick the best move. If we have more than one try to pick middle, if not just take one at random.
            PlayerMove selectedMove = null;
            if (bestMoves.Count > 1)
            {
                // Check for a middle value, if we have one play it
                foreach (PlayerMove move in bestMoves)
                {
                    if (move.MacroX == 1 && move.MacroY == 1)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 4 && move.MacroY == 1)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 7 && move.MacroY == 1)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 1 && move.MacroY == 4)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 4 && move.MacroY == 4)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 7 && move.MacroY == 4)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 1 && move.MacroY == 7)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 4 && move.MacroY == 7)
                    {
                        selectedMove = move;
                        break;
                    }
                    if (move.MacroX == 7 && move.MacroY == 7)
                    {
                        selectedMove = move;
                        break;
                    }
                }

                // If we didn't find a center, pick one at random
                if (selectedMove == null)
                {
                    Random rand = new Random((int)DateTime.Now.Ticks);
                    selectedMove = bestMoves[rand.Next(0, bestMoves.Count)];
                }
            }

            // If we don't have a move yet pick the first one.
            if (selectedMove == null)
            {
                selectedMove = bestMoves.First<PlayerMove>();
            }

            return selectedMove;
        }

        private void ComputeMoveBreadth(BreadthNode initalNodeRoot, ref int[] moveScoreList)
        {
            // Setup the lists.
            BreadthNode currentList = initalNodeRoot;
            BreadthNode buildingList = null;

            int itteration = 0;
            // Main logic loop.
            // Loop while we have a current list and the itteration is low enough.
            while(currentList != null && itteration < 7)
            {
                // Loop through all of the moves we have
                while(currentList != null)
                {
                    //
                    // Make the move
                    //
                    currentList.Slots[currentList.MacroMoveX][currentList.MacroMoveY] = currentList.MovePlayerId;

                    //
                    // Update the macroboard
                    //

                    // Get the cord of the move in the microblock.
                    int microBoardCordX = (int)currentList.MacroMoveX % 3;
                    int microBoardCordY = (int)currentList.MacroMoveY % 3;

                    int sendToMicroBoardIndex = microBoardCordX + microBoardCordY * 3;

                    // Figure out what our replace value is, if the board is won we will replace with -1.
                    sbyte newReplaceValue = (currentList.MacroboardState[sendToMicroBoardIndex] == 1 || currentList.MacroboardState[sendToMicroBoardIndex] == 2) ? (sbyte)-1 : (sbyte)0;

                    // Loop through the values
                    for (int c = 0; c < 9; c++)
                    {
                        // If the board isn't won
                        if (currentList.MacroboardState[c] != 1 && currentList.MacroboardState[c] != 2)
                        {
                            // If we are at the board we are looking for.
                            if (c == sendToMicroBoardIndex)
                            {
                                currentList.MacroboardState[c] = -1;
                            }
                            else
                            {
                                // Set the default value
                                currentList.MacroboardState[c] = newReplaceValue;
                            }
                        }
                    }

                    m_numberOfMovesChecked++;

                    //PrintBoardState(ref currentList.Slots, currentList.MacroboardState, itteration);

                    // Now find the new list of possible moves
                    for(int macro = 0; macro < 9; macro++)
                    {
                        // If it is playable...
                        if(currentList.MacroboardState[macro] == -1)
                        {
                            // Compute the offset.
                            int xOffset = (int)macro % 3 * 3;
                            int yOffset = (int)Math.Floor(macro / 3.0) * 3;

                            // Loop through the spaces
                            for (int y = 0; y < 3; y++)
                            {
                                for (int x = 0; x < 3; x++)
                                {
                                    int macroX = x + xOffset;
                                    int macroY = y + yOffset;

                                    // If the slot is playable...
                                    if(currentList.Slots[macroX][macroY] == 0)
                                    {

                                        //
                                        // We found a new move
                                        //

                                        // Make a new node
                                        BreadthNode newNode = new BreadthNode();

                                        // Copy the slots.
                                        newNode.Slots = new sbyte[9][];
                                        for (int c = 0; c < 9; c++)
                                        {
                                            newNode.Slots[c] = new sbyte[9];
                                            Array.Copy(currentList.Slots[c], newNode.Slots[c], 9);
                                        }

                                        // Copy the macroboard state
                                        newNode.MacroboardState = new sbyte[9];
                                        Array.Copy(currentList.MacroboardState, newNode.MacroboardState, 9);

                                        // Set the initial move index
                                        newNode.InitalMoveRoot = currentList.InitalMoveRoot;

                                        // Set the move new move.
                                        newNode.MacroMoveX = (sbyte)macroX;
                                        newNode.MacroMoveY = (sbyte)macroY;
                                        newNode.MovePlayerId = (sbyte)(currentList.MovePlayerId == 1 ? 2 : 1);

                                        // Set the node into the building list
                                        if (buildingList == null)
                                        {
                                            buildingList = newNode;
                                        }
                                        else
                                        {
                                            newNode.Next = buildingList;
                                            buildingList = newNode;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // We are done, move to the next node
                    currentList = currentList.Next;
                }

                // We are done with this list, move to the next
                currentList = buildingList;
                buildingList = null;

                // Count the itteration
                itteration++;
            }
        }

        #endregion

        private void PrintBoardState(ref sbyte[][] slots, sbyte[] newMacro, int level)
        {
            // print for debug
            for (int print = 0; print < 9; print++)
            {
                Debug.WriteLine(""+slots[0][print]+" "+slots[1][print]+"  "+slots[2][print]+" | "+slots[3][print]+"  "+slots[4][print]+"  "+slots[5][print]+" | "+slots[6][print]+" "+slots[7][print]+"  "+slots[8][print]+" ");
                if (print == 2 || print == 5)
                { 
                    Debug.WriteLine("----------------------------------------------");
                }
            }
            Debug.WriteLine("----------------------------------------------");
            Debug.WriteLine("Macro State: "+newMacro[0]+","+newMacro[1]+","+newMacro[2]+","+newMacro[3]+","+newMacro[4]+","+newMacro[5]+","+newMacro[6]+","+newMacro[7]+","+newMacro[8]+"");
            Debug.WriteLine("Level " + level);
            Debug.WriteLine("");
        }
    }
}
