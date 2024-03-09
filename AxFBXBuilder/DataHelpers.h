#ifndef DATAHELPERS_H
#define DATAHELPERS_H

#include <iostream>
#include <fstream>
#include <string>


class ExportInformation
{
    public:
        double measured_distance;
        double measured_scale;
        bool is_imperial_units;
        double fbx_ratio;
        std::string image_file_path;
};

class PointData
{
    public:
        double fbx_x;
        double fbx_y;
        double rotation;
        int cone_id;
};

class Line
{
    public:
        double fbx_x1;
        double fbx_y1;
        double fbx_x2;
        double fbx_y2;
        double angle;
        int cone_id;
};

#endif