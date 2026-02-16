你好！身为架构师，我已对初版框架进行了深度重构。这份规约（Blueprint）采用了**领域驱动设计（DDD）**的思路，将逻辑解耦，确保程序不仅能跑通，还具备良好的扩展性（比如未来加入 AI 算法或网络对战）。

以下是为你整理的 **《斗地主系统架构设计规约 v2.0》** Markdown 文档。

---

# 斗地主系统架构设计规约 (Standard Specification)

## 1. 核心架构概述

采用 **MVC (Model-View-Controller)** 模式的变体，重点实现“逻辑与表现分离”：

* **Core Models**: 维护游戏实体状态。
* **Logic Engine**: 纯数学运算，处理牌型识别与比较。
* **Controller**: 驱动游戏进程，处理状态机转换。

---

## 2. 数据模型层 (Data Models)

### 2.1 Card (卡牌)

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `id` | Integer | 唯一标识 (0-53) |
| `rank` | Enum | 点数 (3, 4, ..., A, 2, Joker, Big_Joker) |
| `suit` | Enum | 花色 (Spade, Heart, Club, Diamond, None) |
| `weight` | Integer | 权值 (3=3, ..., 2=15, Joker=16, Big_Joker=17) |

### 2.2 Player (玩家)

* **属性**: `id`, `name`, `role` (地主/农夫), `hand_cards` (List), `is_robot` (Boolean)。
* **方法**:
* `add_cards(cards)`: 摸牌并自动排序。
* `remove_cards(cards)`: 出牌。



### 2.3 GameState (游戏上下文)

* **属性**:
* `landlord_id`: 地主索引。
* `last_move`: `Move` 对象（记录最后一次有效出牌及其玩家）。
* `multipliers`: 当前倍率统计（炸弹数、春天等）。
* `phase`: 当前阶段（PREPARING, BIDDING, PLAYING, ENDED）。



---

## 3. 逻辑引擎 (Logic Engine - `RuleEngine`)

这是程序最核心的“黑盒”，不持有状态，只接受输入并返回结果。

### 3.1 牌型定义 (`CardType`)

定义枚举值：`SINGLE` (单张), `PAIR` (对子), `TRIPLE` (三不带), `TRIPLE_WITH_ONE`, `TRIPLE_WITH_TWO`, `STRAIGHT` (顺子), `BOMB` (炸弹), `ROCKET` (王炸) 等。

### 3.2 核心接口

* **`analyze_type(cards)`**:
* **输入**: 玩家选中的卡牌列表。
* **输出**: `{type: CardType, weight: int, length: int}`。如果非法则返回 `None`。


* **`compare_moves(new_move, last_move)`**:
* **逻辑**:
1. 如果 `new_move` 是王炸，必胜。
2. 如果 `new_move` 是炸弹且 `last_move` 不是炸弹，必胜。
3. 如果牌型相同且长度一致，比较 `weight`。




* **`search_available_moves(hand, last_move)`**:
* **用途**: 用于“提示”功能或 AI 出牌。



---

## 4. 游戏流程控制 (Game Controller)

使用 **状态机** 驱动，确保流程不可逆。

### 4.1 核心函数

| 函数 | 阶段 | 逻辑描述 |
| --- | --- | --- |
| `shuffle_and_deal()` | 初始化 | 洗牌，分发 17*3 张牌，留 3 张底牌。 |
| `handle_bid(p_id, score)` | 叫分 | 记录玩家叫分。若满 3 分或三轮结束，确立地主。 |
| `execute_move(p_id, cards)` | 出牌 | 调用 `RuleEngine` 校验。若成功，更新 `last_move` 并切到下一人。 |
| `pass_turn(p_id)` | 出牌 | 玩家选择不出。若连续两人不出，下一个人获得“自由出牌权”。 |
| `calculate_score()` | 结算 | 计算积分：<br> |

---

## 5. 交互接口设计 (Interface/API)

如果是前后端分离或模块化开发，建议定义以下接口：

### 5.1 事件通知 (Server -> Client)

* `on_deal(hand)`: 发牌通知。
* `on_bid_request(p_id)`: 轮到某人叫分。
* `on_move_made(p_id, cards, is_pass)`: 广播某人的动作。
* `on_game_over(winner_id, score_details)`: 游戏结束。

### 5.2 动作请求 (Client -> Server)

* `bid(score)`
* `play_cards(cards)`
* `pass()`

---

## 6. 进阶建议（架构师寄语）

1. **权值抽象**: 不要直接比牌面。将 `3` 设为 `3`，`2` 设为 `15`，这样比较大小时直接 `if card1.weight > card2.weight` 即可。
2. **自由出牌权**: 在代码逻辑中，必须标记当前 `last_move` 是否为空（或已绕场一圈）。如果是，当前玩家可以出任何合法的牌型，不受上家限制。
3. **单元测试**: 强烈建议为 `RuleEngine` 编写单元测试。测试各种边界情况（如连对、飞机带翅膀、四带二），这能省下你后期 80% 的 Debug 时间。

---

这份框架为你搭建了一个稳固的骨架。