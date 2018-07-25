using System;
using UnityEngine;

namespace InvincibleEngine.UnitFramework.Enums {
    public static class TeamColor {
        public static Color GetTeamColor(Team team) {
            switch (team) {
                case Team.Red:
                    return new Color(1f, 0.25f, 0.21f);
                    break;
                case Team.Green:
                    return Color.green;
                    break;
                case Team.Blue:
                    return new Color(0f, 0.64f, 1f);
                    break;
                case Team.Yellow:
                    return Color.yellow;
                    break;
                case Team.Purple:
                    return new Color(0.58f, 0.38f, 1f);
                    break;
                case Team.Orange:
                    return new Color(1f, 0.49f, 0f);
                    break;
                case Team.Pink:
                    return Color.magenta;
                    break;
                case Team.Teal:
                    return Color.cyan;
                    break;
                default:
                    return Color.white;
            }
        }
    }
}