﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicUnit.Logic.GamePageLogic.Games.BombIt;
using LogicUnit.Logic.GamePageLogic.Games.Pacman;
using LogicUnit.Logic.GamePageLogic.Games.Pong.LogicUnit.Logic.GamePageLogic.Games.Pong;
using Objects.Enums;

namespace LogicUnit
{
    public class GameLibrary
    {
        

        public Game CreateAGame(eGames i_GameThatWasPicked)
        {
            Game newGame = null;

            switch (i_GameThatWasPicked)
            {
                case eGames.Pacman:
                    newGame = new Pacman();
                    break;
                case eGames.BombIt:
                    newGame = new BombIt();
                    break;
                case eGames.Pong:
                    newGame = new Pong();
                    break;
            }

            return newGame;
        }
    }
}
