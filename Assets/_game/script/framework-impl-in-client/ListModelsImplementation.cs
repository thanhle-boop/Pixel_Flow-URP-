
using System.Collections.Generic;

public class ListModelsImplementation : IListModelDeclaration
{
    public List<BasePlayerModel> listModels => new List<BasePlayerModel>()
    {
        new CurrencyModel(),
        new LevelModel(),
        new BoosterModel(),
    };
}