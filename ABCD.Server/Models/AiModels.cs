using ABCD.Application;

namespace ABCD.Server.Models {
    public record ChatRequest {
        public required List<AiChatMessage> Messages { get; init; }
    }

    public record AiChatResponse {
        public required string Message { get; init; }
    }

    public record CreatePostFromProposalRequest {
        public required string Title { get; init; }
        public required string Path { get; init; }
        public required List<ProposedFragment> Fragments { get; init; }
    }
}
