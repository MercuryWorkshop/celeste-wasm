using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Linq;
using System;

public class CreditsHook
{
    private Hook Hook;

    private void Hooker(Action<object, int, string, string[]> orig, object self, int padding, string title, string[] people)
    {
        if (title == "Porting")
        {
            orig(self, padding, title, people.Concat(["r58Playz (WASM)", "velzie (WASM)"]).ToArray());
        }
        else
        {
            orig(self, padding, title, people);
        }
    }

    public CreditsHook(Assembly celeste)
    {
        var Credits = celeste.GetType("Celeste.Credits");
        var Thanks = Credits.GetNestedType("Thanks", BindingFlags.NonPublic);
        var constructor = Thanks.GetConstructor([typeof(int), typeof(string), typeof(string[])]);
        Hook = new(constructor, Hooker);
    }
}
