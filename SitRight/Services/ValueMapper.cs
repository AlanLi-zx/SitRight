using SitRight.Models;

namespace SitRight.Services;

public class ValueMapper
{
    private readonly int _hintStartLevel;
    private readonly int _urgentLevel;
    // 新增：存储校准基准角
    private int? _normalAngle;
    private int? _slouchAngle;

    public ValueMapper(int hintStartLevel = 30, int urgentLevel = 80)
    {
        _hintStartLevel = hintStartLevel;
        _urgentLevel = urgentLevel;
    }

    // 新增：更新校准基准角的方法
    public void UpdateCalibration(int normalAngle, int slouchAngle)
    {
        _normalAngle = normalAngle;
        _slouchAngle = slouchAngle;
    }

    public OverlayState Map(int blurLevel)
    {
        // ?? 核心修复：先做区间判断
        // 情况1：未校准 / 角度 ≤ 坐正角（包括后仰）→ 完全不遮罩
        if (!_normalAngle.HasValue || blurLevel <= _normalAngle.Value)
        {
            return OverlayState.FromDisplayLevel(0, _hintStartLevel, _urgentLevel);
        }

        // 情况2：角度在 坐正角 ~ 驼背角 之间 → 线性映射到 0~100 模糊值
        if (_slouchAngle.HasValue && blurLevel < _slouchAngle.Value)
        {
            float ratio = (float)(blurLevel - _normalAngle.Value) / (_slouchAngle.Value - _normalAngle.Value);
            int mappedLevel = (int)(ratio * 100);
            return OverlayState.FromDisplayLevel(mappedLevel, _hintStartLevel, _urgentLevel);
        }

        // 情况3：角度 ≥ 驼背角 → 最大模糊值（100）
        return OverlayState.FromDisplayLevel(100, _hintStartLevel, _urgentLevel);
    }
}
