#include "axFbxBuilder.h"
#define RAPIDJSON_NOMEMBERITERATORCLASS
#include "rapidjson/document.h"
#include "MemoryStream.h"
#include "TrackBuilder.h"
#include <vector>

int BuildMapFbx(char** ppData, int* pLength, const char* json, const char* assetPath, char* errorMessage, int errorMessageSize)
{
    try
    {
        rapidjson::Document doc;
        doc.Parse(json);
        ExportInformation exportInfo;
        std::vector<PointData> points;
        Line startLine;
        Line finishLine;

        ParseJson(doc, exportInfo, points, startLine, finishLine);
        MemoryStream* ms = BuildTrack(assetPath, exportInfo, points, startLine, finishLine);

        const std::vector<char>& data = ms->GetDataVector();
        *pLength = static_cast<int>(data.size());
        *ppData = new char[*pLength];
        std::copy(data.begin(), data.end(), *ppData);

        ms->ManualClose();
        return 0;
    }
    catch (std::exception& e)
    {
        strcpy_s(errorMessage, errorMessageSize, e.what());
        return -1;
    }
}