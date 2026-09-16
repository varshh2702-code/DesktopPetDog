namespace DesktopPetDog;

public static class MessageLibrary
{
    // Each category deliberately produces 125 unique, all-age entries (5 x 5 x 5).
    private static IReadOnlyList<string> Build(string[] a, string[] b, string[] c) =>
        (from x in a from y in b from z in c select $"{x} {y} {z}").Distinct().ToArray();

    public static readonly IReadOnlyList<string> CleanJokes = Build(
        ["Why did the pencil smile?", "Why did the clock relax?", "Why did the book grin?", "Why did the lamp feel bright?", "Why did the bicycle pause?"],
        ["It had a good point.", "It needed a second.", "It found its next chapter.", "It had a brilliant idea.", "It was two-tired."],
        ["That made the whole room laugh!", "A neat little twist!", "No batteries of humour required!", "That is today's tiny giggle!", "Comedy mission complete!"]);
    public static readonly IReadOnlyList<string> DadJokes = Build(
        ["I only know twenty-five letters.", "I ordered a chicken and an egg online.", "I used to dislike facial hair.", "I told my suitcase there would be no trip.", "I wondered why the ball grew larger."],
        ["I do not know y.", "I will let you know which comes first.", "Then it grew on me.", "Now it is dealing with emotional baggage.", "Then it hit me."],
        ["Classic dad-joke energy!", "Please insert a polite groan!", "That one has excellent shelf life!", "Maximum pun achieved!", "The punchline has landed safely!"]);
    public static readonly IReadOnlyList<string> DogJokes = Build(
        ["What is a dog's favourite homework?", "Why did the puppy sit in the shade?", "What did the dog say to the tree?", "Why was the dog a great musician?", "What is a puppy's favourite instrument?"],
        ["A lab report!", "It did not want to be a hot dog!", "Bark!", "It had perfect pitch!", "The trom-bone!"],
        ["Paws for applause!", "That joke was fetching!", "A tail-wagging punchline!", "No ruff feelings!", "Certified good-dog comedy!"]);
    public static readonly IReadOnlyList<string> SchoolJokes = Build(
        ["Why was the maths book worried?", "Why did the student bring a ladder?", "Why was the computer cold?", "Why did the ruler win an award?", "Why did the music note study?"],
        ["It had too many problems.", "The lesson was on another level.", "It left its Windows open.", "It always measured up.", "It wanted a higher grade."],
        ["Class dismissed—with a smile!", "That earns full marks for fun!", "A+ punchline!", "Study break approved!", "The teacher of jokes is proud!"]);
    public static readonly IReadOnlyList<string> Positive = Build(
        ["A small step", "Your steady effort", "A curious question", "A patient restart", "One brave attempt"],
        ["can brighten", "is building", "can unlock", "is shaping", "can improve"],
        ["this moment.", "real progress.", "a useful idea.", "tomorrow's confidence.", "the next try."]);
    public static readonly IReadOnlyList<string> Motivation = Build(
        ["Start with one manageable part;", "Take a calm breath;", "Keep the next goal small;", "Let progress be imperfect;", "Give this task ten focused minutes;"],
        ["your effort", "your patience", "your determination", "your practice", "your curiosity"],
        ["moves you forward.", "adds up.", "makes hard things clearer.", "is worth continuing.", "can surprise you."]);
    public static readonly IReadOnlyList<string> Compliments = Build(
        ["The effort you are showing", "Your willingness to learn", "The patience in your approach", "Your creative thinking", "Your courage to try again"],
        ["shows", "reflects", "demonstrates", "builds", "reveals"],
        ["real determination.", "strong problem-solving.", "meaningful progress.", "valuable curiosity.", "a kind and thoughtful attitude."]);

    public static IReadOnlyList<string> ByCategory(string category) => category switch
    {
        "clean" => CleanJokes, "dad" => DadJokes, "dog" => DogJokes, "school" => SchoolJokes,
        "positive" => Positive, "motivation" => Motivation, "compliment" => Compliments, _ => Positive
    };
}
