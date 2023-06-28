﻿namespace LogicUnit.Logic.GamePageLogic.LiteNet;

public class PlayerData
{
    public PlayerData(int i_PlayerNumber)
    {
        PlayerNumber = i_PlayerNumber;
    }

    public int PlayerNumber { get; init; }
    public int Button { get; set; }
}