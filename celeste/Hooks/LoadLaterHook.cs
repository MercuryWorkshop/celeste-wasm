using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Reflection;
using System;

public class LoadLaterHook
{
    private ILHook Hook;

    private void Hooker(ILContext il)
    {
        ILCursor c = new(il);
        if (!c.TryGotoNext(MoveType.Before, i => { return i.Match(OpCodes.Ldsfld) && ((FieldReference)i.Operand).Name == "IsGGP"; }))
        {
            throw new Exception("Failed to find Celeste IsGGP");
        }

        c.Instrs[c.Index] = Instruction.Create(OpCodes.Ldc_I4_1);
    }

    public LoadLaterHook(Assembly celeste)
    {
        var Settings = celeste.GetType("Celeste.Celeste");
        Hook = new(Settings.GetMethod("LoadContent", BindingFlags.Instance | BindingFlags.NonPublic), Hooker);
    }
}
