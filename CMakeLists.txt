cmake_minimum_required(VERSION 3.19)
project(SampSharpRakNet LANGUAGES CXX VERSION 1.0.0)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(NOT DEFINED OMP_SDK_DIR OR OMP_SDK_DIR STREQUAL "")
    if(EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/../openmp-sampsharp-x64-poc/external/sdk/CMakeLists.txt")
        set(OMP_SDK_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../openmp-sampsharp-x64-poc/external/sdk")
    elseif(EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/external/open.mp-sdk/CMakeLists.txt")
        set(OMP_SDK_DIR "${CMAKE_CURRENT_SOURCE_DIR}/external/open.mp-sdk")
    else()
        message(FATAL_ERROR "open.mp SDK not found. Set -DOMP_SDK_DIR=<path>.")
    endif()
endif()
message(STATUS "SampSharp.RakNet: using open.mp SDK at ${OMP_SDK_DIR}")

if(DEFINED COMPONENTS_DIR AND NOT COMPONENTS_DIR STREQUAL "")
    set(OUTDIR "${COMPONENTS_DIR}")
else()
    set(OUTDIR "${CMAKE_BINARY_DIR}/artifacts")
endif()

foreach(cfg "" "_DEBUG" "_RELEASE" "_RELWITHDEBINFO" "_MINSIZEREL")
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY${cfg} "${OUTDIR}")
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY${cfg} "${OUTDIR}")
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY${cfg} "${OUTDIR}")
endforeach()

add_subdirectory("${OMP_SDK_DIR}" "${CMAKE_BINARY_DIR}/external/sdk" EXCLUDE_FROM_ALL)

if(MSVC)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /std:c++latest /Zc:preprocessor")
endif()

add_library(SampSharp.RakNet SHARED
    native/component.cpp
)

set_target_properties(SampSharp.RakNet PROPERTIES OUTPUT_NAME "SampSharp.RakNet" PREFIX "")
set_property(TARGET SampSharp.RakNet PROPERTY MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

target_compile_definitions(SampSharp.RakNet PRIVATE
    WIN32_LEAN_AND_MEAN
    VC_EXTRALEAN
    NOGDI
)

if(WIN32)
    target_compile_definitions(SampSharp.RakNet PRIVATE NOMINMAX)
endif()

target_link_libraries(SampSharp.RakNet PRIVATE OMP-SDK)
