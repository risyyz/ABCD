using ABCD.Domain;

namespace ABCD.Application
{
    public record AddFragmentCommand(int PostId, int AfterFragmentId, FragmentType FragmentType, string Version);
}
