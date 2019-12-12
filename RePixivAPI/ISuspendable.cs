namespace RePixivAPI
{
    public interface ISuspendable
    {
        void SuspendHttpClients();

        void ResumeHttpClients();
    }
}