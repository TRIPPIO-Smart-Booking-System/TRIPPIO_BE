//using Quartz;
//using Trippio.Core.Services;

//namespace Trippio.Data.Service.Jobs;

//public class OrderPostProcessJob : IJob
//{
//    private readonly IEmailService _email;
//    public OrderPostProcessJob(IEmailService email) => _email = email;

//    public async Task Execute(IJobExecutionContext context)
//    {
        
//        await _email.SendAsync("user@example.com", "Your receipt", "Thanks for your order!");
//    }
//}
