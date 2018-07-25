using System;
using UnityEngine;

namespace InvincibleEngine.UnitFramework.Enums {
    public static class TeamUtility {
        public static Color GetTeamColor(Team team) {
            switch (team) {
                case Team.Red:
                    return Color.red;
                    break;
                case Team.Green:
                    return Color.green;
                    break;
                case Team.Blue:
                    return Color.blue;
                    break;
                case Team.Yellow:
                    return Color.yellow;
                    break;
                case Team.Purple:
                    return new Color(0.5f, 0f, 1f);
                    break;
                case Team.Orange:
                    return new Color(1f, 0.33f, 0f);
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