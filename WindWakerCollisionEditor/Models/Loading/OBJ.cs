﻿using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;

namespace WindWakerCollisionEditor
{
    class OBJ : IModelSource
    {
        ObservableCollection<Category> m_catList;
        List<Vector3> m_verts;

        public OBJ()
        {
            m_catList = new ObservableCollection<Category>();
            m_verts = new List<Vector3>();
        }

        public IEnumerable<Category> Load(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                Category curCat = null;
                Group curGroup = null;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line.Length == 0)
                        continue;

                    string[] decompLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    switch(decompLine[0])
                    {
                        case "v":
                            m_verts.Add(new Vector3(Convert.ToSingle(decompLine[1]), 
                                                    Convert.ToSingle(decompLine[2]), 
                                                    Convert.ToSingle(decompLine[3])));
                            break;
                        case "f":
                            Triangle newTri = GetTriangle(decompLine);
                            newTri.ParentGroup = curGroup;
                            curGroup.Triangles.Add(newTri);
                            break;
                        case "o":
                        case "g":
                            Category tempCat = new Category(decompLine[1]);
                            Group grp = new Group(decompLine[1]);
                            grp.GroupCategory = tempCat;
                            tempCat.Groups.Add(grp);
                            curCat = tempCat;
                            curGroup = grp;
                            m_catList.Add(curCat);
                            break;
                        default:
                            break;
                    }
                }

                // We have to generate rendering stuff for each group
                foreach (Category cat in m_catList)
                {
                    foreach (Group grp in cat.Groups)
                        grp.CreateBufferObjects();
                }
            }

            return m_catList;
        }

        private Triangle GetTriangle(string[] source)
        {
            Triangle tri = new Triangle();
            Vector3[] triVerts = new Vector3[3];

            for (int i = 1; i < 4; i++)
            {
                string vert = source[i];

                if (vert.Contains('/'))
                {
                    string[] decompVert = vert.Split('/');
                    triVerts[i - 1] = m_verts[Convert.ToInt32(decompVert[0]) - 1];
                }

                else
                {
                    triVerts[i - 1] = m_verts[Convert.ToInt32(vert) - 1];
                }
            }

            tri.Vertex1 = triVerts[0];
            tri.Vertex2 = triVerts[1];
            tri.Vertex3 = triVerts[2];

            return tri;
        }
    }
}
