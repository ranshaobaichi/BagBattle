using System;
using UnityEngine;

namespace Assets.BagBattles.Types
{
    #region TriggerType
    [Serializable]
    public enum FireTriggerType
    {
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
    }

    // 时间触发器具体类型
    public enum TimeTriggerType
    {
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
    }

    // 其他触发器具体类型
    public enum ByOtherTriggerType
    {
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
    }
    #endregion

    #region 食物
    [Serializable]
    public enum FoodType
    {
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
    }
    #endregion

    #region 子弹种类
    public enum BulletType
    {
        // 后面带Single的都是单发子弹类型
        Normal_Bullet_Single,
        Spear_Bullet_Single,
        Rocket_Bullet_Single,
        Lightning_Bullet_Single,
        Fire_Bullet_Single,
        Ice_Bullet_Single,
        Swallow_Bullet_Single,
        Bomb_Bullet_Single,
        Jump_Bullet_Single,
        Split_Bullet_Single,
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
    }
    #endregion

    #region 环绕物
    public enum SurroundType
    {
        SingleFireBall, // 单个火球
        SingleElectricityBall,  // 单个电球
    }
    #endregion

    #region 其他种类
    public enum OtherType
    {
        FireZone,
        D100,
        FallingPillar,
        NextShootDamageUp1, // 下一发子弹伤害加成
        NextShootDamageUp2,
        NextShootDamageUp3,
    }
    #endregion
}