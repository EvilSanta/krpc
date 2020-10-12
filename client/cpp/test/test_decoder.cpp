#include <google/protobuf/stubs/port.h>

#include <gtest/gtest-message.h>
#include <gtest/gtest-test-part.h>

#include <string>

#include "gtest/gtest.h"

#include "krpc/decoder.hpp"
#include "krpc/krpc.pb.hpp"
#include "krpc/platform.hpp"

#include "services/test_service.hpp"

namespace pb = google::protobuf;

TEST(test_decoder, test_decode_message) {
  std::string message = "0a0b536572766963654e616d65120d50726f6365647572654e616d65";
  krpc::schema::ProcedureCall call;
  krpc::decoder::decode(call, krpc::platform::unhexlify(message));
  ASSERT_EQ("ServiceName", call.service());
  ASSERT_EQ("ProcedureName", call.procedure());
}

TEST(test_decoder, test_decode_value) {
  unsigned int value;
  krpc::decoder::decode(value, krpc::platform::unhexlify("ac02"));
  ASSERT_EQ(300, value);
}

TEST(test_decoder, test_decode_unicode_string) {
  std::string value;
  krpc::decoder::decode(value, krpc::platform::unhexlify("03e284a2"));
  ASSERT_EQ(krpc::platform::unhexlify("e284a2"), value);
}

TEST(test_decoder, test_decode_size) {
  std::string message = "1c";
  pb::uint32 size = krpc::decoder::decode_size(krpc::platform::unhexlify(message));
  ASSERT_EQ(28, size);
}

TEST(test_decoder, test_decode_class) {
  krpc::services::TestService::TestClass object;
  krpc::decoder::decode(object, krpc::platform::unhexlify("ac02"));
  ASSERT_EQ(krpc::services::TestService::TestClass(nullptr, 300), object);
}

TEST(test_decoder, test_decode_class_none) {
  krpc::services::TestService::TestClass object;
  krpc::decoder::decode(object, krpc::platform::unhexlify("00"));
  ASSERT_EQ(krpc::services::TestService::TestClass(), object);
}

TEST(test_decoder, test_guid) {
  ASSERT_EQ(
    "6f271b39-00dd-4de4-9732-f0d3a68838df",
    krpc::decoder::guid(krpc::platform::unhexlify("391b276fdd00e44d9732f0d3a68838df")));
}
