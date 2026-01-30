using System.Drawing;
using System.Windows.Forms;

namespace ALsSoundSwitcher
{
  internal class MenuStripColorTable : ProfessionalColorTable
  {
    public Color ColorMenuBorder = Color.FromArgb(61, 61, 67);
    public Color ColorMenuItemSelected = Color.FromArgb(76, 76, 77);
    public Color ColorBackground = Color.FromArgb(43, 43, 43);
    public Color ColorSeparator = Color.FromArgb(61, 61, 67);
    public Color ColorStatusStripGradient = Color.FromArgb(234, 237, 241);
    public Color ColorButtonSelected = Color.FromArgb(88, 146, 226);
    public Color ColorButtonPressed = Color.FromArgb(110, 160, 230);

    public void SetColours(ColourPack colourPack)
    {
      if (!colourPack.ColorMenuBorder.IsEmpty) ColorMenuBorder = colourPack.ColorMenuBorder;
      if (!colourPack.ColorMenuItemSelected.IsEmpty) ColorMenuItemSelected = colourPack.ColorMenuItemSelected;
      if (!colourPack.ColorBackground.IsEmpty) ColorBackground = colourPack.ColorBackground;
      if (!colourPack.ColorSeparator.IsEmpty) ColorSeparator = colourPack.ColorSeparator;
      if (!colourPack.ColorStatusStripGradient.IsEmpty) ColorStatusStripGradient = colourPack.ColorStatusStripGradient;
      if (!colourPack.ColorButtonSelected.IsEmpty) ColorButtonSelected = colourPack.ColorButtonSelected;
      if (!colourPack.ColorButtonPressed.IsEmpty) ColorButtonPressed = colourPack.ColorButtonPressed;
    }

    public override Color ToolStripDropDownBackground => ColorBackground;
    public override Color MenuStripGradientBegin => ColorBackground;
    public override Color MenuStripGradientEnd => ColorBackground;
    public override Color CheckBackground => ColorBackground;
    public override Color CheckPressedBackground => ColorBackground;
    public override Color CheckSelectedBackground => ColorBackground;
    public override Color MenuItemSelected => ColorMenuItemSelected;
    public override Color ImageMarginGradientBegin => ColorBackground;
    public override Color ImageMarginGradientMiddle => ColorBackground;
    public override Color ImageMarginGradientEnd => ColorBackground;
    public override Color MenuItemBorder => ColorMenuItemSelected;
    public override Color MenuBorder => ColorMenuBorder;
    public override Color SeparatorDark => ColorSeparator;
    public override Color SeparatorLight => ColorSeparator;
    public override Color StatusStripGradientBegin => ColorStatusStripGradient;
    public override Color StatusStripGradientEnd => ColorStatusStripGradient;
    public override Color ButtonSelectedGradientBegin => ColorButtonSelected;
    public override Color ButtonSelectedGradientMiddle => ColorButtonSelected;
    public override Color ButtonSelectedGradientEnd => ColorButtonSelected;
    public override Color ButtonSelectedBorder => ColorButtonSelected;
    public override Color ButtonPressedGradientBegin => ColorButtonPressed;
    public override Color ButtonPressedGradientMiddle => ColorButtonPressed;
    public override Color ButtonPressedGradientEnd => ColorButtonPressed;
    public override Color ButtonPressedBorder => ColorButtonPressed;
  }
}
