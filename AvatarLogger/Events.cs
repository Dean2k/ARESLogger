namespace AvatarLogger
{
    class Events
    {
        //Requirements for unlimited favorites to commit working
        internal interface OnUIEvent
        {
            void UI();
        }
        public interface OnUpdateEvent
        {
            void OnUpdate();
        }
    }
}
