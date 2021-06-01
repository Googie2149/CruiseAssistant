using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace CruiseAssistant.Preconditions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PTNSWineRunnersAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo commandInfo, IServiceProvider service)
        {
            var mContext = context as MinitoriContext;

            if (mContext == null || !mContext.IsHelp)
                return Task.FromResult(PreconditionResult.FromSuccess());

            if (mContext.Guild == null)
                return Task.FromResult(PreconditionResult.FromError("This command requires a guild to run."));

            var user = mContext.User as SocketGuildUser;

            if (user.Id == 102528327251656704)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else if (mContext.Guild.Id == 800080948716503040)  // Pilots Trade Network
            {
                var sommelier = mContext.Guild.GetRole(838520893181263872);
                var admin = mContext.Guild.GetRole(800125148971663392);
                var mod = mContext.Guild.GetRole(813814494563401780);

                if (user.Roles.Contains(sommelier) || user.Roles.Contains(admin) || user.Roles.Contains(mod))
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                    return Task.FromResult(PreconditionResult.FromError("You don't have permission to run this command."));
            }
            else if (mContext.Guild.Id == 818174236480897055) // P.T.N. Test Server
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("This command cannot be run in this guild."));
        }
    }
}
