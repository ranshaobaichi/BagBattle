using System;
using UnityEngine;

namespace Assets.BagBattles.Types
{
    #region TriggerType
    [Serializable]
    public enum FireTriggerType
    {
        反应强化连接I,
        反应强化连接II,
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
    }

    // 时间触发器具体类型
    public enum TimeTriggerType
    {
        标准压入装置I,
        标准压入装置II,
        强力压入装置I,
        快速压入装置I,
        起爆装置,
        强效引爆装置,
        Type1,
    }

    // 其他触发器具体类型
    public enum ByOtherTriggerType
    {
        延伸触发装置I,
        延伸触发装置II,
        连锁触发装置,
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
        小麦,
        奇怪海洋触须,
        应急口粮,
        温热稠液,
        食尸鬼的礼物,
        蠕动幼崽肉,
        腐臭的鲜肉,
        水银饮剂,
        Type1,
        锻铁,
        液态金属,
        陨铁,
        绯色晶体,
        火药袋,
        重型弹药,
        献祭匕首,
        奇怪头骨挂坠,
        旧的假的甜甜圈,
        回火的军用水壶,
        战斗手套,
        快速枪口,
        弹鼓,
        变异的指南针,
        变异的旅行手杖,
        红外线,
        伸缩枪口,
        瞄准镜,
        树脂,
        箭毒蛙提取液,
        干冰,
        血肉冰冻术残页,
        爆裂附魔术残页,
        冰冻附魔术残页,
        赤阳稳固术残页,
        两级稳固术残页,
        金属强化器,
        额外撞针,
        Type2,
        Type3,
    }
    #endregion

    #region 子弹种类
    public enum BulletType
    {
        // 后面带Single的都是单发子弹类型
        标准子弹,
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