using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Abodit.Graph
{
    /// <summary>
    /// A predicate relationship between two nodes in a graph, e.g. parent, child, antonym, synonym, ...
    /// </summary>
    /// <remarks>
    /// Graphs can use any type you want as the predicate (edge) type, this one is provided for convenience
    /// and includes some of the more common predicates used in RDF.
    /// </remarks>
    [DebuggerDisplay("{Name}")]
    public class Relation : IEquatable<Relation>, IRelation
    {
        /// <summary>
        /// Name of relation
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Relation is reflexive
        /// </summary>
        public bool IsReflexive { get; set; }

        private Relation(string name)
        {
            this.Name = name;
        }

        private static readonly Dictionary<string, Relation> identityMap = new Dictionary<string, Relation>();

        /// <summary>
        /// Get an identity-mapped relation by name
        /// </summary>
        public static Relation GetByName(string name, bool isReflexive = false)
        {
            if (!identityMap.TryGetValue(name.ToUpperInvariant(), out Relation? result))
            {
                result = new Relation(name) { IsReflexive = isReflexive };
                identityMap.Add(name.ToUpperInvariant(), result);
            }
            return result;
        }

        /// <summary>
        /// The Antonym relation
        /// </summary>
        public static readonly Relation Antonym = GetByName("antonym", true);

        /// <summary>
        /// The synonym relation
        /// </summary>
        public static readonly Relation Synonym = GetByName("synonym", true);

        /// <summary>
        /// The Member Meronym Of relation
        /// </summary>
        public static readonly Relation MemberMeronymOf = GetByName("memberMeronymOf");        // mereonym

        /// <summary>
        /// The PartMeronymOf relation
        /// </summary>
        public static readonly Relation PartMeronymOf = GetByName("partMeronymOf");        // mereonym

        /// <summary>
        /// The HolynymOf relation
        /// </summary>
        public static readonly Relation HolonymOf = GetByName("holonymOf");      // holonym

        /// <summary>
        /// The SimilarTo relation
        /// </summary>
        public static readonly Relation SimilarTo = GetByName("similarTo");

        /// <summary>
        /// Classified by topic: points to another Noun
        /// </summary>
        public static readonly Relation ClassifiedByTopic = GetByName("classifiedByTopic");

        /// <summary>
        /// Classified by region (points to a geographic region)
        /// </summary>
        public static readonly Relation ClassifiedByRegion = GetByName("classifiedByRegion");

        /// <summary>
        /// Classified by usage
        /// </summary>
        public static readonly Relation ClassifiedByUsage = GetByName("classifiedByUsage");

        /// <summary>
        /// PertainsTo
        /// </summary>
        public static readonly Relation PertainsTo = GetByName("pertainsTo");

        /// <summary>
        /// Derivationally related, e.g. China, chinese
        /// </summary>
        public static readonly Relation DerivationallyRelated = GetByName("derivationallyRelated");

        /// <summary>
        /// Related (but not derivationally), e.g. knives and forks
        /// </summary>
        public static readonly Relation Related = GetByName("related");

        /// <summary>
        /// Domain is used to describe another predicate, i.e. what can go on its left hand side
        /// </summary>
        public static readonly Relation Domain = GetByName("domain");

        /// <summary>
        /// Range is used to describe another predicate, i.e. what can go on its right hand side
        /// </summary>
        public static readonly Relation Range = GetByName("range");

        /// <summary>
        /// Points to a word node for a synset
        /// </summary>
        public static readonly Relation WordFor = GetByName("wordFor");

        /// <summary>
        /// Points to a noun form
        /// </summary>
        public static readonly Relation NounForm = GetByName("noun");

        /// <summary>
        /// Points to a verb form
        /// </summary>
        public static readonly Relation VerbForm = GetByName("verb");

        /// <summary>
        /// Points to an adjective form
        /// </summary>
        public static readonly Relation AdjectiveForm = GetByName("adjective");

        /// <summary>
        /// Points to a positive adjective form (obsolete - positive and negative are context dependent)
        /// </summary>
        public static readonly Relation AdjectivePositive = GetByName("adjectivePositive");

        /// <summary>
        /// Points to a negative adjective form (obsolete - positive and negative are context dependent)
        /// </summary>
        public static readonly Relation AdjectiveNegative = GetByName("adjectiveNegative");

        /// <summary>
        /// Points to a superlative form
        /// </summary>
        public static readonly Relation SuperlativeForm = GetByName("superlative");

        /// <summary>
        /// Points to a comparative form
        /// </summary>
        public static readonly Relation ComparativeForm = GetByName("comparative");

        /// <summary>
        /// Points to an adverb form
        /// </summary>
        public static readonly Relation AdverbForm = GetByName("adverb");

        /// <summary>
        /// Points to a past tense
        /// </summary>
        public static readonly Relation PastTense = GetByName("pastTense");

        /// <summary>
        /// Points to a future tense
        /// </summary>
        public static readonly Relation FutureTense = GetByName("futureTense");

        /// <summary>
        /// Points to a past participle
        /// </summary>
        public static readonly Relation PastParticiple = GetByName("pastParticple");

        /// <summary>
        /// Points to a present participle (gerund)
        /// </summary>
        public static readonly Relation PresentParticiple = GetByName("presentParticiple");

        /// <summary>
        /// Points to a fisrt person singular word form
        /// </summary>
        public static readonly Relation FirstPersonSingular = GetByName("firstPersonSingular");

        /// <summary>
        /// Points to a fisrt person plural word form
        /// </summary>
        public static readonly Relation FirstPersonPlural = GetByName("firstPersonPlural");

        /// <summary>
        /// Points to a singular word form
        /// </summary>
        public static readonly Relation SingularForm = GetByName("singular");

        /// <summary>
        /// Points to a plural word form
        /// </summary>
        public static readonly Relation PluralForm = GetByName("plural");

        /// <summary>
        /// Points to a collective noun word form
        /// </summary>
        public static readonly Relation CollectiveNoun = GetByName("collectiveNoun");

        /// <summary>
        /// Points to a parent type (e.g. lion is a mammal)
        /// </summary>
        public static readonly Relation RDFSType = GetByName("type");

        /// <summary>
        /// Compares two Relations for equality (reference equals since identity mapped)
        /// </summary>
        public bool Equals(Relation? other) => ReferenceEquals(this, other);

        /// <summary>
        /// Returns the name of this relation as an arrow text
        /// </summary>
        public override string ToString() => $"--{this.Name}-->";

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Relation r && ReferenceEquals(this, r);

        /// <inheritdoc />
        public override int GetHashCode() => this.Name.GetHashCode();
    }
}