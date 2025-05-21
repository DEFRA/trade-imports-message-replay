using Microsoft.AspNetCore.Builder;

namespace TradeImportsMessageReplay.Test.Config;

public class EnvironmentTest
{

   [Fact]
   public void IsNotDevModeByDefault()
   { 
       var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
       var isDev = TradeImportsMessageReplay.Config.Environment.IsDevMode(builder);
       Assert.False(isDev);
   }
}
