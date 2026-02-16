namespace FightLord.Core.Enums
{
    public enum CardType
    {
        Unknown = 0,
        Single,         // 单张
        Pair,           // 对子
        Triple,         // 三张
        TripleWithOne,  // 三带一
        TripleWithTwo,  // 三带二
        Straight,       // 顺子
        PairChain,      // 连对
        Plane,          // 飞机
        PlaneWithWings, // 飞机带翅膀
        FourWithTwo,    // 四带二
        Bomb,           // 炸弹
        Rocket          // 王炸
    }
}
