using System.Dynamic;

namespace CDev;

public class GlobleVariables 
{
    public GlobleVariables(dynamic value)
    {
        Value = value;
    }

    public dynamic Value { get; private set; }
    
    public static GlobleVariables CompFile { get { return new GlobleVariables("C:\\Coding\\CDev\\src\\cs\\CDev\\CDev\\Content\\src\\main.cd"); } }

    public static void Set(GlobleVariables v, dynamic value)
    {
        v.Value = value;
    }
}