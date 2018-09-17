using System;
using UnityEngine;


namespace InvincibleEngine.UnitFramework.Enums{

    //Team enumerator
    [Flags] public enum PlayerTeam {
        Red = 1,
        Green = 2,
        Blue = 4,
        Yellow = 8,
        Purple = 16,
        Orange = 32,
        Pink = 64,
        Teal = 128
    }

    //Implicit convertion
    public static class ETeamConvert {
        public static Color EColor(this PlayerTeam team) {
            switch (team) {
                case PlayerTeam.Red:
                    return new Color(1f, 0.25f, 0.21f);
                case PlayerTeam.Green:
                    return Color.green;
                case PlayerTeam.Blue:
                    return new Color(0f, 0.64f, 1f);
                case PlayerTeam.Yellow:
                    return Color.yellow;
                case PlayerTeam.Purple:
                    return new Color(0.58f, 0.38f, 1f);
                case PlayerTeam.Orange:
                    return new Color(1f, 0.49f, 0f);
                case PlayerTeam.Pink:
                    return Color.magenta;
                case PlayerTeam.Teal:
                    return Color.cyan;
                default:
                    return Color.white;
            }
        }

    }

}