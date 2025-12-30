using Fidelity.Domain.Entities;
using System.Linq.Expressions;

namespace Fidelity.Domain.Specifications;

public class ClienteAttivoSpecification : Specification<Cliente>
{
    public override Expression<Func<Cliente, bool>> ToExpression()
    {
        return c => !c.IsDeleted;
    }
}

public class ClienteConPuntiSufficientiSpecification : Specification<Cliente>
{
    private readonly int _puntiRichiesti;

    public ClienteConPuntiSufficientiSpecification(int puntiRichiesti)
    {
        _puntiRichiesti = puntiRichiesti;
    }

    public override Expression<Func<Cliente, bool>> ToExpression()
    {
        return c => c.PuntiDisponibili >= _puntiRichiesti;
    }
}

public class ClienteByFidelityCodeSpecification : Specification<Cliente>
{
    private readonly string _code;

    public ClienteByFidelityCodeSpecification(string code)
    {
        _code = code;
    }

    public override Expression<Func<Cliente, bool>> ToExpression()
    {
        return c => c.CodiceFidelity == _code;
    }
}
