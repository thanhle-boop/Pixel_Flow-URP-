
using System.Collections.Generic;

public class ListConfigsImplementation : IListConfigDeclaration
{
    public List<IBaseConfig> listConfigs => new()
    {
        new ShopConfig(),
    };
}