/*
 * This file is a part of Pocket Heroes Game project
 * 	http://www.pocketheroes.net
 *	https://code.google.com/p/pocketheroes/
 *
 * Copyright 2004-2010 by Robert Tarasov and Anton Stuk (iO UPG)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */ 

#include "stdafx.h"
#include "../../externals/lzokay/lzokay.hpp"
#include "lzo.h"

namespace LZO {

const uint16 LZO_BLOCK_HDR = 'L' | ('Z'<<8);
const uint32 LZO_MAX_BUF_LEN = 4 * 1024 * 1024;

bool Init()
{
	return true;
}

lzokay::Dict<> dict;

uint32 Compress(const unsigned char* rawBuff, uint32 rawBuffLen, iDynamicBuffer& lzoBuff)
{
	check(rawBuffLen > 0 && rawBuffLen <= LZO_MAX_BUF_LEN);
	uint32 out_len = lzokay::compress_worst_size(rawBuffLen);
	unsigned char* buff = new unsigned char[out_len];

	lzokay::EResult res = lzokay::compress(rawBuff,rawBuffLen,buff,out_len,out_len,dict);
    if (res != lzokay::EResult::Success) {
        // this should NEVER happen
		delete[] buff;
		check(0);
        return 0;
    }

	lzoBuff.Write(LZO_BLOCK_HDR);
	lzoBuff.Write(rawBuffLen);
	lzoBuff.Write(buff, out_len);
	delete[] buff;
	return lzoBuff.GetSize();
}


uint32 Decompress(const unsigned char* lzoBuff, uint32 lzoBuffLen, iDynamicBuffer& rawBuff)
{
	uint16 hdr;
	memcpy(&hdr, lzoBuff, sizeof(uint16));
	lzoBuffLen -=  sizeof(uint16);
	lzoBuff += sizeof(uint16);
	if (hdr != LZO_BLOCK_HDR) {
		check(0);
		return 0;
	}

	uint32 raw_buf_len;
	memcpy(&raw_buf_len, lzoBuff, sizeof(uint32));
	lzoBuffLen -=  sizeof(uint32);
	lzoBuff += sizeof(uint32);
	if (raw_buf_len <= 0 || raw_buf_len > LZO_MAX_BUF_LEN) {
		check(0);
		return 0;
	}

	rawBuff.ReInit(raw_buf_len);
	rawBuff.IncSize( raw_buf_len );
	lzokay::EResult res = lzokay::decompress(lzoBuff,lzoBuffLen,(unsigned char*)rawBuff.GetData(),raw_buf_len,raw_buf_len);
	if (res != lzokay::EResult::Success) {
		check(0);
		return 0;
	}
	return raw_buf_len;
}


};
