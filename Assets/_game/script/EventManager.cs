using System;

public class EventManager
{
    public static Action OnStartGame;
    public static Action OnWinGame;
    public static Action OnLoseGame;
    public static Action<PigComponent> OnClickPig;
    public static Action<PigComponent> OnPigEnterQueue;
    public static Action OnBlockDestroyed;
    public static Action<PigComponent> OnPigDestroyed;
    public static Action OnQueueFull;
    public static Action OnQueueNotFull;

    public static Action OnFullConveyorSlot;
    public static Action OnContinueGame;
    public static Action OnJumpToConveyor;

    public static Action<PigComponent> OnPigOutOfAmmo;
    public static Action<PigComponent> OnPigIsOnTopNoMoreHidden;
    public static Action<PigComponent> OnClearLinked;

    public static Action OnUseAddTray;
    public static Action OnUseHand;
    public static Action OnEndHand;

    public static Action OnUseShuffle;

    public static Action OnUseSuperCat;
    public static Action<string> OnClickBlock;

    public static Action<int, int> OnItemCountChanged;

}
