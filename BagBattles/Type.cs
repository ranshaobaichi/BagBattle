using System;
using UnityEngine;

namespace Assets.BagBattles.Types
{
    #region TriggerType
    [Serializable]
    /// <summary>
    /// 开火次数触发器具体类型
    /// </summary>
    public enum FireTriggerType
    {
        /// <summary>
        /// 开火一次触发，范围为邻近1格
        /// </summary>
        Name_1_1,
        Type2,
        Type3,
    }

    public enum TimeTriggerType
    {
        /// <summary>
        /// 1s触发，范围为邻近1格
        /// </summary>
        Type1,
        Type2,
        Type3,
    }
    #endregion

    #region FoodType
    [Serializable]
    public enum FoodType
    {
        None,
        Type1,
        Type2,
        Type3,
    }
    #endregion

    #region BulletType
    public enum BulletType
    {
        None,
        Normal_Bullet_Single,
        Spear_Bullet_Single,
        Rocket_Bullet_Single,
        Lightning_Bullet_Single,
        Fire_Bullet_Single,
        Ice_Bullet_Single,
        Swallow_Bullet_Single,
        Bomb_Bullet_Single,
        Jump_Bullet_Single,
        Split_Bullet_Single
    }
    #endregion
}