using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;

public class BloomHook
{
    private ILHook Bloom;
    private Hook BufferCreate;

    public static object Buffer;

    private MethodInfo GameplayBuffersCreate;

    private void BufferCreateHook(Action orig)
    {
        orig();
        Buffer = GameplayBuffersCreate.Invoke(null, [320, 180]);
    }

    private void BloomHooker(ILContext il)
    {
        ILCursor c = new(il);
        if (!c.TryGotoNext(MoveType.Before, i => i.Match(OpCodes.Ldsfld)))
        {
            throw new Exception("Failed to find BloomRenderer tempA");
        }
        var buf = il.Module.ImportReference(typeof(BloomHook).GetField("Buffer", BindingFlags.Static | BindingFlags.Public));
        c.Instrs[c.Index].Operand = buf;
    }

    public BloomHook(Assembly celeste)
    {
        var GameplayBuffers = celeste.GetType("Celeste.GameplayBuffers");
        GameplayBuffersCreate = GameplayBuffers.GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic);
        var gameplayBuffersInit = GameplayBuffers.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);

        var Bloom = celeste.GetType("Celeste.BloomRenderer");
        var bloomApply = Bloom.GetMethod("Apply", BindingFlags.Public | BindingFlags.Instance);

        BufferCreate = new Hook(gameplayBuffersInit, BufferCreateHook);
        this.Bloom = new ILHook(bloomApply, BloomHooker);
    }
}
