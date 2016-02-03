using RandToeEngine.CommonObjects;
using RandToeEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.Bots
{
    public class ThinkDeep : IPlayer
    {
        sbyte m_myPlayValue;
        sbyte m_theirPlayValue;

        int numberOfMovesChecked = 0;


        public void MoveRequested(IPlayerBase playerBase)
        {
            //  Setup
            m_myPlayValue = playerBase.PlayerId;
            m_theirPlayValue = m_myPlayValue == 1 ? (sbyte)2 : (sbyte)1;

            // Look for a block

            // Check for a win

            // Pick a space


            sbyte[][] slots = playerBase.CurrentBoard.Slots;
            PlayerMove move = ComputeMove(ref slots, playerBase.CurrentBoard.MacroBoardStates);
            playerBase.MakeMove(move);
        }


        private PlayerMove ComputeMove(ref sbyte[][] slots, sbyte[] macroboard)
        {
            int bestScore = int.MinValue;
            List<PlayerMove> bestMoves = new List<PlayerMove>();

            // Loop through each macroboard we can play on.
            for(int i = 0; i < 9; i++)
            {
                // If it is playable...
                if(macroboard[i] == -1)
                {
                    // Compute the offset.
                    int xOffset = (int)i % 3 * 3;
                    int yOffset = (int)Math.Floor(i / 3.0) * 3;

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
                                // Make the move
                                slots[macroX][macroY] = m_myPlayValue;

                                // Update the macro
                                sbyte[] newMacroBoard = new sbyte[9];
                                Array.Copy(macroboard, newMacroBoard, 9);

                                // If we didn't win the game we need to update the macro boards, figure out where we were sent.  
                                int sendToMicroBoardIndex = x + y * 3;

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

                                numberOfMovesChecked = 0;
                                DateTime begin = DateTime.Now;

                                // Make moves
                                int score = RecursiveMove(ref slots, newMacroBoard, false, 0, 4);

                                PlayerBase.Logger.Log(this, $"score ({score}) moves checked ({numberOfMovesChecked}) time ({(DateTime.Now - begin).TotalMilliseconds}) time per move ({(DateTime.Now - begin).TotalMilliseconds / numberOfMovesChecked})");

                                // If this is equal to the current top score pick at random.
                                if(score == bestScore)
                                {
                                    bestMoves.Add(new PlayerMove(macroX, macroY));
                                    bestScore = score;
                                }
                                // If this score is better clear the past list and add this one.
                                else if(score > bestScore)
                                {
                                    bestMoves.Clear();
                                    bestMoves.Add(new PlayerMove(macroX, macroY));
                                    bestScore = score;
                                }

                                // Reset the board
                                slots[macroX][macroY] = 0;
                            }                            
                        }
                    }
                }
            }

            // Pick the first move.
            PlayerMove selectedMove = null;

            // If we have many pick one at random. This will help make us less predictable.
            if(bestMoves.Count > 1)
            {
                // Check for a center value, if we have one play it
                foreach(PlayerMove move in bestMoves)
                {
                    if(move.MacroX == 1 && move.MacroY == 1)
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
            if(selectedMove == null)
            {
                selectedMove = bestMoves.First<PlayerMove>();
            }

            return selectedMove;
        }

        DateTime lastTime = DateTime.Now;
        double average = 0;

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
            for (int i = 0; i < 9; i++)
            {
                // If it is playable
                if (macroboard[i] == -1)
                {
                    // Compute the offset.
                    int xOffset = (int)i % 3 * 3;
                    int yOffset = (int)Math.Floor(i / 3.0) * 3;

                    // Loop through the spaces
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            int macroX = x + xOffset;
                            int macroY = y + yOffset;

                            // If the space is free...
                            if (slots[macroX][macroY] == 0)
                            {
                                numberOfMovesChecked++;
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

                                bool hasMacroWinner = false;
                                if (microWinFound)
                                {
                                    // If we have a winner now set it.
                                    newMacroBoard[i] = playedValue;

                                    // Update the score for this round, 
                                    // just a small indication that this is good or bad.
                                    scoreForMove += playedValue == m_myPlayValue ? 1 : -1;

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
                                        // Someone won!
                                        PrintBoardState(ref slots, newMacroBoard, currentDepth);

                                        // Update the score to indicate someone won.
                                        scoreForMove += playedValue == m_myPlayValue ? 10 : -10;

                                        // Reset the macroboard slot
                                        slots[macroX][macroY] = 0;

                                        // Return now, since they would make this move.
                                        return scoreForMove;
                                    }
                                }

                                // If we didn't win the game we need to update the macro boards, figure out where we were sent.  
                                int sendToMicroBoardIndex = x + y * 3;

                                // Figure out what our replace value is, if the board is won we will replace with -1.
                                sbyte newReplaceValue = (newMacroBoard[sendToMicroBoardIndex] == 1 || newMacroBoard[sendToMicroBoardIndex] == 2) ? (sbyte)-1 : (sbyte)0;

                                // Loop through the values
                                for (int c = 0; c < 9; c++)
                                {
                                    // If the board isn't won
                                    if(newMacroBoard[c] != 1 && newMacroBoard[c] != 2)
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
                            }
                        }
                    }
                }
            }

            // When we are done return the score.
            return scoreForMove;
        }

        private void PrintBoardState(ref sbyte[][] slots, sbyte[] newMacro, int level)
        {
            // print for debug
            for (int print = 0; print < 9; print++)
            {
                Debug.WriteLine($"{slots[0][print]} {slots[1][print]}  {slots[2][print]} | {slots[3][print]}  {slots[4][print]}  {slots[5][print]} | {slots[6][print]} {slots[7][print]}  {slots[8][print]} ");
                if (print == 2 || print == 5)
                {
                    Debug.WriteLine("----------------------------------------------");
                }
            }
            Debug.WriteLine("----------------------------------------------");
            Debug.WriteLine($"Macro State: {newMacro[0]},{newMacro[1]},{newMacro[2]},{newMacro[3]},{newMacro[4]},{newMacro[5]},{newMacro[6]},{newMacro[7]},{newMacro[8]}");
            Debug.WriteLine("Level " + level);
            Debug.WriteLine("");
        }
    }
}
