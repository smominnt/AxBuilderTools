#ifndef TRACKBUILDER_H
#define TRACKBUILDER_H

#include <fbxsdk.h>
#define RAPIDJSON_NOMEMBERITERATORCLASS
#include "rapidjson/document.h" 
#include <fstream>
#include <iostream>
#include <filesystem>
#include <sstream>
#include <vector>
#include <windows.h>
#include <shlobj.h>
#include "DataHelpers.h"
#include "MemoryStream.h"

namespace fs = std::filesystem;

void ParseJson(const rapidjson::Document& doc, ExportInformation& exportInfo, std::vector<PointData>& points, Line& startLine, Line& finishLine);

void LoadScene(FbxManager* pSdkManager, FbxScene* pScene, std::string filePath);

MemoryStream* BuildTrack(const char* assetPath, ExportInformation exportInfo, std::vector<PointData> points, Line startLine, Line finishLine);

#endif // TRACKBUILDER_H