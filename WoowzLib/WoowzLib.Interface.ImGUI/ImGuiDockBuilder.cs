using System.Runtime.InteropServices;
using ImGuiNET;

namespace WoowzLib.Interface.ImGUI;

public static class ImGuiDockBuilder{
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void igDockBuilderRemoveNode(uint node_id);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void igDockBuilderAddNode(uint node_id, ImGuiDockNodeFlags flags);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void igDockBuilderSetNodeSize(uint node_id, System.Numerics.Vector2 size);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint igDockBuilderSplitNode(uint node_id, ImGuiDir split_dir, float size_ratio, out uint out_id_at_dir, out uint out_id_at_opposite_dir);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void igDockBuilderDockWindow(string window_name, uint node_id);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void igDockBuilderFinish(uint node_id);
}