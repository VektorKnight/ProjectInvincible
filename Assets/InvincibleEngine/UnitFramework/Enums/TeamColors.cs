﻿using System;
using UnityEngine;

namespace InvincibleEngine.UnitFramework.Enums {
    public static class TeamColor {
        public static Color GetTeamColor(ETeam team) {
            switch (team) {
                case ETeam.Red:
                    return new Color(1f, 0.25f, 0.21f);
                    break;
                case ETeam.Green:
                    return Color.green;
                    break;
                case ETeam.Blue:
                    return new Color(0f, 0.64f, 1f);
                    break;
                case ETeam.Yellow:
                    return Color.yellow;
                    break;
                case ETeam.Purple:
                    return new Color(0.58f, 0.38f, 1f);
                    break;
                case ETeam.Orange:
                    return new Color(1f, 0.49f, 0f);
                    break;
                case ETeam.Pink:
                    return Color.magenta;
                    break;
                case ETeam.Teal:
                    return Color.cyan;
                    break;
                default:
                    return Color.white;
            }
        }
    }
}