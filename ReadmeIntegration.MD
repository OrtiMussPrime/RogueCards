## RogueCards Integration Framework

### Core Components

1. **GameplayCardController** - Bridge between your card animation system and the dungeon gameplay
   - Subscribes to card events from your existing system
   - Handles card placement in the dungeon
   - Manages visual effects for card playing

2. **CardDataComponent** - Attaches to your existing Card class to add gameplay functionality
   - Stores CardData for gameplay effects
   - Updates card visuals based on card type

3. **DiceController** - Dice visualization using the same animation style as your cards
   - Uses DOTween for smooth animations
   - Provides visual feedback with shakes and punches

4. **CardAnimationIntegration** - Helper class for connecting animations to gameplay events
   - Creates visual effects for path creation
   - Animates grid highlights
   - Provides feedback for card placement

### Gameplay Systems

1. **GameManager** - Coordinates all gameplay systems
   - Manages game state (Exploring, Combat, etc.)
   - Connects player, dungeon, and card systems

2. **PlayerController** - Controls the player character in the dungeon
   - Handles movement with animations
   - Manages player stats and effects

3. **CombatManager** - Manages turn-based combat
   - Uses dice rolling for attack resolution
   - Provides visual feedback for combat actions

4. **CardFactory** - Creates different card types
   - Manages card pools
   - Deals cards to the player

### Card System

1. **CardData** - Base class for all card data
   - Scriptable objects for different card types
   - Contains card effects and properties

2. **PathCardData** - Creates paths in the dungeon
   - Defines available movement directions
   - Visual feedback for path creation

3. **CombatCardData** - Used in combat
   - Dice modifiers for attacks
   - Combat effects

4. **SpecialCardData** - Special effects
   - Teleportation, healing, etc.
   - Visual feedback for special effects

### Integration Points

1. **Card Events** - Your existing Card class events are used for gameplay
   - BeginDragEvent -> Show valid placements
   - EndDragEvent -> Place card in dungeon
   - SelectEvent -> Card selection effects

2. **Visual Effects** - All gameplay actions use DOTween animations similar to your cards
   - Dice rolling
   - Enemy effects
   - Grid cell highlights

3. **Card Placement** - When a card is played:
   - Card animation plays
   - Effect is applied to the dungeon
   - Visual feedback occurs on affected grid cells

### Next Steps

1. **Implement Grid Cell Visuals** - Create visuals for different cell types (path, enemy, entrance/exit)

2. **Create Card Prefabs** - Make prefabs for different card types with appropriate visuals

3. **Add UI Elements** - Health bars, mana, deck/discard piles

4. **Refine Animations** - Further tune animation parameters to match your existing style

5. **Event System** - Add Unity Events for gameplay events to make connections more flexible

6. **Audio System** - Add sound effects for cards, dice, combat