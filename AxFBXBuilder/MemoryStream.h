#ifndef MEMORYSTREAM_H
#define MEMORYSTREAM_H

#include <fbxsdk.h>
#include <iostream>
#include <vector>

class MemoryStream : public FbxStream
{
public:
	MemoryStream(std::vector<char>& buffer) : mData(buffer), mPosition(0) {}

	~MemoryStream()
	{
		ManualClose();
	}

	virtual EState GetState()
	{
		return mData.empty() ? FbxStream::eOpen : FbxStream::eClosed;
	}

	virtual bool Open(void* /*pStreamData*/)
	{
		mPosition = 0;
		return true;
	}

	virtual bool Close()
	{
		return true;
	}

	bool ManualClose()
	{
		mData.clear();
		return true;
	}

	virtual size_t Write(const void* pData, FbxUInt64 pSize)
	{
		const char* charData = static_cast<const char*>(pData);
		if (mPosition + pSize > mData.size())
		{
			mData.resize(mPosition + pSize);
		}
		std::copy(charData, charData + pSize, mData.begin() + mPosition);
		mPosition += pSize;
		return pSize;
	}

	virtual size_t Read(void* pData, FbxUInt64 pSize) const
	{
		if (mPosition + pSize > mData.size())
		{
			pSize = mData.size() - mPosition;
		}
		std::copy(mData.begin() + mPosition, mData.begin() + mPosition + pSize, static_cast<char*>(pData));
		mPosition += pSize;
		return pSize;
	}


	virtual void Seek(const FbxInt64& pOffset, const FbxFile::ESeekPos& pSeekPos)
	{
		switch (pSeekPos)
		{
		case FbxFile::eBegin:
			mPosition = pOffset;
			break;
		case FbxFile::eCurrent:
			mPosition += pOffset;
			break;
		case FbxFile::eEnd:
			mPosition = mData.size() - pOffset;
			break;
		}
	}

	virtual FbxInt64 GetPosition() const
	{
		return mPosition;
	}

	virtual void SetPosition(FbxInt64 pPosition)
	{
		mPosition = pPosition;
	}

	const std::vector<char>& GetDataVector() const
	{
		return mData;
	}

	// not implemented
	virtual int GetError() const { return 0; }

	// not implemented
	virtual void ClearError() { return; }

	// not implemented
	virtual int GetReaderID() const { return 0; }

	// not implemented
	virtual int GetWriterID() const { return 0; }

	// not implemented
	virtual bool Flush() { return true; }

private:
	std::vector<char> mData;
	mutable size_t mPosition;
};

#endif // MEMORYSTREAM_H


/*
class MemoryStream : public FbxStream
{
public:
	MemoryStream(std::vector<char>& buffer) : mBuffer(buffer), mPosition(0) {}

	virtual EState GetState() override { return eOpen; }

	virtual bool Open(void* /*pStreamData*) override
	{
	mPosition = 0;
	return true;
	}

	virtual bool Close() override { return true; }

	virtual bool Flush() override { return true; }

	virtual size_t Read(void* pData, FbxUInt64 pSize) const override { return 0; }

	virtual char* ReadString(char* pBuffer, int pMaxSize, bool pStopAtFirstWhiteSpace = false) override
	{
		std::memcpy(pBuffer, &mBuffer[mPosition], pMaxSize);
		mPosition += pMaxSize;
		return pBuffer;
	}

	virtual size_t Write(const void* pData, FbxUInt64 pSize) override
	{
		const char* data = static_cast<const char*>(pData);
		mBuffer.insert(mBuffer.end(), data, data + pSize);
		mPosition += pSize;
		return pSize;
	}

	virtual int GetReaderID() const override { return -1; }

	virtual int GetWriterID() const override { return -1; }

	virtual void Seek(const FbxInt64& pOffset, const FbxFile::ESeekPos& pSeekPos) override
	{
		switch (pSeekPos)
		{
		case FbxFile::ESeekPos::eBegin:
			mPosition = pOffset;
			break;
		case FbxFile::ESeekPos::eCurrent:
			mPosition += pOffset;
			break;
		case FbxFile::ESeekPos::eEnd:
			mPosition = mBuffer.size() - pOffset;
			break;
		}
	}

	size_t GetBufferSize()
	{
		return mBuffer.size();
	}

	const char* GetBuffer()
	{
		return mBuffer.data();
	}

	virtual FbxInt64 GetPosition() const override { return mPosition; }

	virtual void SetPosition(FbxInt64 pPosition) override { mPosition = pPosition; }

	virtual int GetError() const override { return 0; }

	virtual void ClearError() override {}

private:
	std::vector<char>& mBuffer;
	long mPosition;
};*/