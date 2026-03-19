public enum PigState
{
    InLane,              // Đang đứng trong hàng
    JumpingToConveyor,   // Đang nhảy lên conveyor
    OnConveyor,          // Đang di chuyển trên conveyor
    MovingToQueue,       // Đang di chuyển về queue
    InQueue,             // Đang ở trong queue
    MovingInQueue,       // Đang di chuyển trong queue (rearrange)
    JumpingFromQueue,    // Đang nhảy từ queue lên conveyor
    Destroying           // Đang bị destroy
}

