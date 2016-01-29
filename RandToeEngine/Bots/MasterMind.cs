using RandToeEngine.CommonObjects;
using RandToeEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandToeEngine.Bots
{
    public class MasterMind : IPlayer
    {
        sbyte myValue = 2;
        sbyte theirValue = 1;


        public void MoveRequested(IGameEngine engine)
        {
            // MacroBoard newboard = engine.CurrentBoard.MakeCopy();

            sbyte[][] slots = engine.CurrentBoard.Slots;
            PlayerMove move = ComputeMove(ref slots, engine.CurrentBoard.MacroBoardStates);
        }


        private PlayerMove ComputeMove(ref sbyte[][] slots, sbyte[] macroboard)
        {
            int bestScore = 0;
            PlayerMove bestMove = null;
            for(int i = 0; i < 9; i++)
            {
                if(macroboard[i] == -1)
                {
                    int xOffset = (int)i % 3 * 3;
                    int yOffset = (int)Math.Floor(i / 3.0) * 3;

                    for (int y = 0; y < 3; y++)
                    {
                        for(int x = 0; x < 3; x++)
                        {
                            int macroX = x + xOffset;
                            int macroY = y + yOffset;

                            if (slots[macroX][macroY] == 0)
                            {
                                // Make the move
                                slots[macroX][macroY] = myValue;

                                // Update the macro
                                sbyte[] newMacro = new sbyte[9];
                                Array.Copy(macroboard, newMacro, 9);

                                if (macroboard[x + y * 3] == 0 || macroboard[x + y * 3] == -1)
                                {
                                    newMacro[x + y * 3] = -1;
                                    newMacro[i] = 0;
                                }
                                else
                                {
                                    for (int c = 0; x < 9; x++)
                                    {
                                        if (newMacro[c] != 1 || newMacro[c] != 2)
                                        {
                                            newMacro[c] = -1;
                                        }
                                    }
                                }

                                // Make moves
                                int score = RecursiveMove(ref slots, newMacro, false, 0);

                                if (score > bestScore)
                                {
                                    bestMove = new PlayerMove(0, macroX, macroY);
                                }

                                // Reset the board
                                slots[macroX][macroY] = 0;
                            }                            
                        }
                    }
                }
            }
            return bestMove;
        }

        private int RecursiveMove(ref sbyte[][] slots, sbyte[] macroboard, bool isMe, int level)
        {

           // RandToeEngineCore.Logger.Log(this,"Starting Level "+level);

            int scoreForMove = 0;
            for (int i = 0; i < 9; i++)
            {
                if (macroboard[i] == -1)
                {
                    int xOffset = (int)i % 3 * 3;
                    int yOffset = (int)Math.Floor(i / 3.0) * 3;

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            int macroX = x + xOffset;
                            int macroY = y + yOffset;

                            if (slots[macroX][macroY] == 0)
                            {
                                // Make the move
                                slots[macroX][macroY] = (sbyte)(isMe ? myValue : theirValue);

                                // Update the macro
                                sbyte[] newMacro = new sbyte[9];
                                Array.Copy(macroboard, newMacro, 9);

                                sbyte winValue = 0;
                                // Check row wins
                                if (slots[xOffset][yOffset] == slots[xOffset + 1][yOffset] && slots[xOffset + 1][yOffset] == slots[xOffset + 2][yOffset])
                                {
                                    if(winValue == 0)
                                    {
                                        winValue = slots[xOffset][yOffset];
                                    }
                                }
                                if (slots[xOffset][yOffset + 1] == slots[xOffset + 1][yOffset + 1] && slots[xOffset + 1][yOffset + 1] == slots[xOffset + 2][yOffset + 1])
                                {
                                    if (winValue == 0)
                                    {
                                        winValue = slots[xOffset][yOffset + 1];
                                    }
                                }
                                if (slots[xOffset][yOffset + 2] == slots[xOffset + 1][yOffset] + 2 && slots[xOffset + 1][yOffset + 2] == slots[xOffset + 2][yOffset + 2])
                                {
                                    if (winValue == 0)
                                    {
                                        winValue = slots[xOffset][yOffset + 2];
                                    }
                                }

                                // Check col wins
                                if (slots[xOffset][yOffset] == slots[xOffset][yOffset + 1] && slots[xOffset][yOffset + 1] == slots[xOffset][yOffset + 2])
                                {
                                    if (winValue == 0)
                                    {
                                        winValue = slots[xOffset][yOffset];
                                    }
                                }
                                if (slots[xOffset + 1][yOffset] == slots[xOffset + 1][yOffset + 1] && slots[xOffset + 1][yOffset + 1] == slots[xOffset + 1][yOffset + 2])
                                {
                                    if (winValue == 0)
                                    {
                                        winValue = slots[xOffset + 1][yOffset];
                                    }
                                }
                                if (slots[xOffset + 2][yOffset] == slots[xOffset + 2][yOffset + 1] && slots[xOffset + 2][yOffset + 1] == slots[xOffset + 2][yOffset + 2])
                                {
                                    if (winValue == 0)
                                    {
                                        winValue = slots[xOffset + 2][yOffset];
                                    }
                                }

                                // check diag
                                if (slots[xOffset][yOffset] == slots[xOffset + 1][yOffset + 1] && slots[xOffset + 1][yOffset + 1] == slots[xOffset + 2][yOffset + 2])
                                {
                                    if (winValue == 0)
                                    {
                                        winValue = slots[xOffset][yOffset];
                                    }
                                }
                                if (slots[xOffset][yOffset + 2] == slots[xOffset + 1][yOffset + 1] && slots[xOffset + 1][yOffset + 1] == slots[xOffset + 2][yOffset])
                                {
                                    if (winValue == 0)
                                    {
                                        winValue = slots[xOffset][yOffset + 2];
                                    }
                                }

                                if (winValue != 0)
                                {
                                    // If we have a winner now set it.
                                    newMacro[i] = winValue;
                                }

                                sbyte macroWin = 0;
                                // Check row wins
                                if (newMacro[0] == newMacro[1] && newMacro[1] == newMacro[2])
                                {
                                    if (macroWin == 0 || macroWin == -1)
                                    {
                                        macroWin = newMacro[0];
                                    }
                                }
                                if (newMacro[3] == newMacro[4] && newMacro[4] == newMacro[5])
                                {
                                    if (macroWin == 0 || macroWin == -1)
                                    {
                                        macroWin = newMacro[3];
                                    }
                                }
                                if (newMacro[6] == newMacro[7] && newMacro[7] == newMacro[8])
                                {
                                    if (macroWin == 0 || macroWin == -1)
                                    {
                                        macroWin = newMacro[6];
                                    }
                                }


                                // Check col wins
                                if (newMacro[0] == newMacro[3] && newMacro[3] == newMacro[6])
                                {
                                    if (macroWin == 0 || macroWin == -1)
                                    {
                                        macroWin = newMacro[0];
                                    }
                                }
                                if (newMacro[1] == newMacro[4] && newMacro[4] == newMacro[7])
                                {
                                    if (macroWin == 0 || macroWin == -1)
                                    {
                                        macroWin = newMacro[1];
                                    }
                                }
                                if (newMacro[2] == newMacro[5] && newMacro[5] == newMacro[8])
                                {
                                    if (macroWin == 0 || macroWin == -1)
                                    {
                                        macroWin = newMacro[2];
                                    }
                                }

                                // check diag
                                if (newMacro[0] == newMacro[4] && newMacro[4] == newMacro[8])
                                {
                                    if (macroWin == 0 || macroWin == -1)
                                    {
                                        macroWin = newMacro[0];
                                    }
                                }
                                if (newMacro[2] == newMacro[4] && newMacro[4] == newMacro[6])
                                {
                                    if (macroWin == 0 || macroWin == -1)
                                    {
                                        macroWin = newMacro[2];
                                    }
                                }

                                if (macroWin != 0 && macroWin != -1)
                                {
                                    // Someone won!
                                    scoreForMove += macroWin == myValue ? 1 : -1;
                                }
                                else
                                {
                                    
                                    // Now figure out where to go, if the new post is -1 or 0 go there.
                                    if (macroboard[x + y * 3] == 0 || macroboard[x + y * 3] == -1)
                                    {
                                        newMacro[x + y * 3] = -1;

                                        // Clear this if not won.
                                        if (newMacro[i] == -1 && i != (x + y * 3))
                                        {
                                            newMacro[i] = 0;
                                        }
                                    }
                                    else
                                    {
                                        // If the new location is won set all not one spaces to -1
                                        for (int c = 0; c < 9; c++)
                                        {
                                            if (newMacro[c] != 1 && newMacro[c] != 2)
                                            {
                                                newMacro[c] = -1;
                                            }
                                        }
                                    }

                                    // Make the move
                                    scoreForMove += RecursiveMove(ref slots, newMacro, !isMe, ++level);
                                }

                                // Reset the board
                                slots[macroX][macroY] = 0;
                            }
                        }
                    }
                }
            }
            return scoreForMove;
        }
    }
}
