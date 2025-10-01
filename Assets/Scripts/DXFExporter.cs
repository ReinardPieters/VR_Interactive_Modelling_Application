using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine;

public class DXFExporter : MonoBehaviour
{
    public StrokeStore store;
    static readonly CultureInfo CI = CultureInfo.InvariantCulture;

    public void SaveDXF()
    {
        if (store == null || store.polylines.Count == 0) return;

        var sb = new StringBuilder();
        sb.Append("0\nSECTION\n2\nENTITIES\n");

        foreach (var pl in store.polylines)
        {
            sb.Append("0\nPOLYLINE\n8\n" + pl.layer + "\n66\n1\n70\n" + (pl.closed ? 1 : 0) + "\n");
            foreach (var p in pl.ptsMM)
            {
                sb.Append("0\nVERTEX\n8\n" + pl.layer + "\n10\n" + p.x.ToString(CI) + "\n20\n" + p.y.ToString(CI) + "\n30\n0.0\n");
            }
            sb.Append("0\nSEQEND\n");
        }

        sb.Append("0\nENDSEC\n0\nEOF\n");
        var path = Path.Combine(Application.persistentDataPath, "design.dxf");
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log("DXF saved: " + path);
    }
}
