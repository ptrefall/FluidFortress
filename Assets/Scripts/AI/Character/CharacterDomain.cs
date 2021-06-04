using FluidHTN;

namespace Fluid.AI.Character
{
    public static class CharacterDomain
    {
        public static Domain<CharacterContext> Create(string domainName)
        {
            return new CharacterDomainBuilder(domainName)
                .AddBehaviour()
                .Build();
        }
    }
}