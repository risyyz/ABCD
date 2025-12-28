using ABCD.Domain;
using ABCD.Infra.Data;

namespace ABCD.Application {
    public record RequestContext(Blog Blog, ApplicationUser ApplicationUser); 
}
